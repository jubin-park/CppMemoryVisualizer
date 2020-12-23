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
    class GoCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        private readonly MainViewModel mMainViewModel;

        public GoCommand(MainViewModel mainViewModel)
        {
            mMainViewModel = mainViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return mMainViewModel.ProcessCdbOrNull != null && mMainViewModel.ThreadCdbOrNull != null;
        }

        public void Execute(object parameter)
        {
            mMainViewModel.LastInstruction = EDebugInstructionState.GO;
            mMainViewModel.SendInstruction(CdbInstructionSet.GO);
        }
    }
}
