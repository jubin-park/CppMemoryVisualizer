using CppMemoryVisualizer.ViewModels;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace CppMemoryVisualizer.Commands
{
    class DebugCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        private readonly MainViewModel mMainViewModel;

        public DebugCommand(MainViewModel mainViewModel)
        {
            mMainViewModel = mainViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return mMainViewModel.SourcePathOrNull != null;
        }

        public void Execute(object parameter)
        {
            Debug.Assert(mMainViewModel.SourcePathOrNull != null);

            mMainViewModel.ShutdownCdb();

            string dirPath = Path.GetDirectoryName(mMainViewModel.SourcePathOrNull);
            string fileName = Path.GetFileName(mMainViewModel.SourcePathOrNull);
            string fileNameOnly = Path.GetFileNameWithoutExtension(mMainViewModel.SourcePathOrNull);

            App app = Application.Current as App;

            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = app.CdbPath;
            processInfo.WorkingDirectory = dirPath;
            processInfo.Arguments = $"-o {fileNameOnly}.exe -y {fileNameOnly}.pdb -srcpath {fileName}";
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardInput = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;

            mMainViewModel.ProcessCdbOrNull = new Process();
            mMainViewModel.ProcessCdbOrNull.StartInfo = processInfo;
            mMainViewModel.ExecuteCdb();
        }
    }
}
