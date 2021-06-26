using CppMemoryVisualizer.ViewModels;
using CppMemoryVisualizer.Models;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows;
using System.Text.RegularExpressions;

namespace CppMemoryVisualizer.Commands
{
    sealed class LoadSourceFileCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        private static readonly string[] STANDARD_CPP_OPTIONS = { "-std=c++11", " -std=c++14", " -std=c++17" };

        private readonly MainViewModel mMainViewModel;

        public LoadSourceFileCommand(MainViewModel mainViewModel)
        {
            mMainViewModel = mainViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "C++ 소스 파일 (*.cpp)|*.cpp";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = Path.GetFileName(openFileDialog.FileName);
                if (fileName.Contains(' '))
                {
                    MessageBox.Show("파일 이름에 공백문자가 들어갈 수 없습니다.", App.WINDOW_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (mMainViewModel.BreakPointListOrNull != null && mMainViewModel.BreakPointListOrNull.Count > 0
                    && MessageBoxResult.Yes != MessageBox.Show("새 파일을 불러올 경우 설정한 중단점이 모두 사라집니다. 계속 진행하시겠습니까?", App.WINDOW_TITLE, MessageBoxButton.YesNo, MessageBoxImage.Exclamation))
                {
                    return;
                }

                string dirPath = Path.GetDirectoryName(openFileDialog.FileName);
                string fileNameOnly = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                mMainViewModel.SourcePathOrNull = openFileDialog.FileName;

                mMainViewModel.ShutdownGdb();

                bool hasMallocHeader = false;

                uint lineCount = 1;
                {
                    string line;
                    TextReader reader = new StreamReader(openFileDialog.FileName);

                    Regex rx = new Regex(@"^\s*#include\s*<(stdlib.h|cstdlib|malloc.h)>");

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!hasMallocHeader)
                        {
                            Match match = rx.Match(line);
                            if (match.Success)
                            {
                                hasMallocHeader = true;
                            }
                        }
                        lineCount++;
                    }
                    reader.Close();
                }

                if (!hasMallocHeader)
                {
                    ++lineCount;
                    File.WriteAllText(openFileDialog.FileName, "#include <malloc.h> /* auto-generated */" + Environment.NewLine + File.ReadAllText(openFileDialog.FileName));
                    //MessageBox.Show("힙 메모리 분석을 위해 헤더 <malloc.h> 를 자동으로 추가합니다.", App.WINDOW_TITLE, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                mMainViewModel.BreakPointListOrNull = new BreakPointList(lineCount + 1);
                mMainViewModel.SourceCode = File.ReadAllText(openFileDialog.FileName);
                
                mMainViewModel.LinePointer = 0;
                if (null != mMainViewModel.CallStackOrNull)
                {
                    mMainViewModel.CallStackOrNull.Clear();
                }
                if (null != mMainViewModel.HeapManagerOrNull)
                {
                    mMainViewModel.HeapManagerOrNull.Heaps.Clear();
                }

                // execute gcc compiler
                ProcessStartInfo processInfo = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    WorkingDirectory = dirPath,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                Process process = new Process();
                process.StartInfo = processInfo;

                process.OutputDataReceived += p_OutputDataReceived;
                process.ErrorDataReceived += p_ErrorDataReceived;

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.StandardInput.WriteLine(string.Format("del \"{0}.{1}\"", fileNameOnly, "exe"));
                process.StandardInput.WriteLine(string.Format("gcc {0} -o {1} -g -lstdc++{2}", fileName, fileNameOnly, STANDARD_CPP_OPTIONS[(uint)mMainViewModel.StandardCppVersion]));
                process.StandardInput.WriteLine(string.Format("dir \"{0}.exe\"", fileNameOnly));
                process.StandardInput.Close();

                process.WaitForExit();
                process.Close();
            }
        }

        private void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}
