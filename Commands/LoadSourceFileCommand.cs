using CppMemoryVisualizer.ViewModels;
using CppMemoryVisualizer.Models;
using Microsoft.Win32;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows;

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

                if (mMainViewModel.BreakPointList != null && mMainViewModel.BreakPointList.Count > 0
                    && MessageBoxResult.Yes != MessageBox.Show("새 파일을 불러올 경우 설정한 중단점이 모두 사라집니다. 계속 진행하시겠습니까?", App.WINDOW_TITLE, MessageBoxButton.YesNo, MessageBoxImage.Exclamation))
                {
                    return;
                }

                string dirPath = Path.GetDirectoryName(openFileDialog.FileName);
                string fileNameOnly = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                mMainViewModel.SourcePathOrNull = openFileDialog.FileName;

                mMainViewModel.ShutdownGdb();

                uint lineCount = 1;
                {
                    string line;
                    TextReader reader = new StreamReader(openFileDialog.FileName);
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineCount++;
                    }
                    reader.Close();
                }
                mMainViewModel.BreakPointList = new BreakPointList(lineCount + 1);
                mMainViewModel.SourceCode = File.ReadAllText(openFileDialog.FileName);

                // execute gcc compiler
                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.FileName = "cmd.exe";
                processInfo.WorkingDirectory = dirPath;
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardInput = true;

                Process process = Process.Start(processInfo);

                process.StandardInput.WriteLine(string.Format("del \"{0}.{1}\"", fileNameOnly, "exe"));
                process.StandardInput.WriteLine(string.Format("gcc {0} -o {1} -g -lstdc++{2}", fileName, fileNameOnly, STANDARD_CPP_OPTIONS[(uint)mMainViewModel.StandardCppVersion]));
                process.StandardInput.WriteLine(string.Format("dir \"{0}.exe\"", fileNameOnly));
                process.StandardInput.Close();

                process.WaitForExit();
                process.Close();
            }
        }
    }
}
