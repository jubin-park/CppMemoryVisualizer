using CppMemoryVisualizer.ViewModels;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CppMemoryVisualizer.Commands
{
    class DebugCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly MainViewModel mMainViewModel;

        public DebugCommand(MainViewModel mainViewModel)
        {
            mMainViewModel = mainViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;//mMainViewModel.SourcePathOrNull != null &&
                //File.Exists(mMainViewModel.SourcePathOrNull);
        }

        public void Execute(object parameter)
        {
            Debug.WriteLine(mMainViewModel.SourcePathOrNull);
        }
    }
}
