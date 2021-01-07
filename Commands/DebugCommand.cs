using CppMemoryVisualizer.ViewModels;
using CppMemoryVisualizer.Models;
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
    sealed class DebugCommand : ICommand
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
            return mMainViewModel.SourcePathOrNull != null && mMainViewModel.CurrentInstruction == EDebugInstructionState.STANDBY || mMainViewModel.CurrentInstruction == EDebugInstructionState.DEAD;
        }

        public void Execute(object parameter)
        {
            Debug.Assert(mMainViewModel.SourcePathOrNull != null);

            mMainViewModel.CurrentInstruction = EDebugInstructionState.DEBUG;

            mMainViewModel.ShutdownCdb();
            mMainViewModel.ExecuteCdb();

            mMainViewModel.CurrentInstruction = EDebugInstructionState.STANDBY;
        }
    }
}
