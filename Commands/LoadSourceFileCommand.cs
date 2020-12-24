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
using CppMemoryVisualizer.ViewModels;

namespace CppMemoryVisualizer.Commands
{
    class LoadSourceFileCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

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
            openFileDialog.Filter = "C/C++ 소스 파일 (*.c; *.cpp)|*.c;*.cpp";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                mMainViewModel.ShutdownCdb();

                mMainViewModel.SourcePathOrNull = openFileDialog.FileName;
                string dirPath = Path.GetDirectoryName(openFileDialog.FileName);
                string fileName = Path.GetFileName(openFileDialog.FileName);
                string fileNameOnly = Path.GetFileNameWithoutExtension(openFileDialog.FileName);

                mMainViewModel.SourceCode = File.ReadAllText(openFileDialog.FileName);

                // compile
                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.FileName = "cmd.exe";
                processInfo.WorkingDirectory = dirPath;
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardInput = true;

                Process process = Process.Start(processInfo);

                // Execute MSVC x86 compiler
                {
                    Debug.Write("Loading vcvars32.bat ... ");
                    App app = Application.Current as App;
                    string compilerPath = Path.Combine(app.VsPath, "VC\\Auxiliary\\Build\\vcvars32.bat");

                    if (!File.Exists(compilerPath))
                    {
                        MessageBox.Show("vcvar32.bat 파일을 찾을 수 없습니다.", "caption", MessageBoxButton.OK, MessageBoxImage.Error);
                        Debug.WriteLine("FAILED");
                    }
                    else
                    {
                        process.StandardInput.WriteLine("\"" + compilerPath + "\"");
                        Debug.WriteLine("SUCCESS");
                    }                    
                }

                process.StandardInput.WriteLine(string.Format("del {0}.{1}", fileNameOnly, "exe"));
                process.StandardInput.WriteLine(string.Format("del {0}.{1}", fileNameOnly, "exe"));
                process.StandardInput.WriteLine(string.Format("del {0}.{1}", fileNameOnly, "ilk"));
                process.StandardInput.WriteLine(string.Format("del {0}.{1}", fileNameOnly, "obj"));
                process.StandardInput.WriteLine(string.Format("del {0}.{1}", fileNameOnly, "pdb"));
                process.StandardInput.WriteLine(string.Format("cl /std:c++17 /EHsc /Zi {0}", fileName));
                process.StandardInput.WriteLine(string.Format("dir {0}.*", fileNameOnly));
                process.StandardInput.Close();

                process.WaitForExit();
                process.Close();
            }
        }
    }
}
