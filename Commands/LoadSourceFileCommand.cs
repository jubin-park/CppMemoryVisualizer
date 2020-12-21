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
    class LoadSourceFileCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

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
                string dirName = Path.GetDirectoryName(openFileDialog.FileName);
                string fileName = Path.GetFileName(openFileDialog.FileName);
                string fileNameOnly = Path.GetFileNameWithoutExtension(openFileDialog.FileName);

                // cl 컴파일러 환경변수 등록
                {
                    App app = Application.Current as App;
                    string command = Path.Combine(app.VsPath, "VC\\Auxiliary\\Build\\vcvars32.bat");

                    ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe", "/c \"" + command + "\"");
                    startInfo.CreateNoWindow = false;
                    startInfo.UseShellExecute = false;

                    Process process = Process.Start(startInfo);

                    process.WaitForExit();
                    process.Close();
                }

                // compile
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "cmd.exe";
                    startInfo.WorkingDirectory = dirName;
                    startInfo.CreateNoWindow = false;
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardInput = true;

                    Process process = Process.Start(startInfo);
                    process.StandardInput.WriteLine(string.Format("del {0}.{1}", fileNameOnly, "exe"));
                    process.StandardInput.WriteLine(string.Format("del {0}.{1}", fileNameOnly, "ilk"));
                    process.StandardInput.WriteLine(string.Format("del {0}.{1}", fileNameOnly, "obj"));
                    process.StandardInput.WriteLine(string.Format("del {0}.{1}", fileNameOnly, "pdb"));
                    process.StandardInput.WriteLine(string.Format("cl /EHsc /Zi {0}", fileName));
                    process.StandardInput.WriteLine("dir");
                    process.StandardInput.Close();

                    process.WaitForExit();
                    process.Close();
                }
            }
        }
    }
}
