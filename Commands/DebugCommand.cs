using CppMemoryVisualizer.ViewModels;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace CppMemoryVisualizer.Commands
{
    sealed class DebugCommand : ICommand
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

        private readonly MainViewModel mMainViewModel;

        public DebugCommand(MainViewModel mainViewModel)
        {
            mMainViewModel = mainViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return mMainViewModel.SourcePathOrNull != null && (mMainViewModel.CurrentInstruction == EDebugInstructionState.STANDBY || mMainViewModel.CurrentInstruction == EDebugInstructionState.DEAD);
        }

        public void Execute(object parameter)
        {
            Debug.Assert(mMainViewModel.SourcePathOrNull != null);

            mMainViewModel.CurrentInstruction = EDebugInstructionState.START_DEBUGGING;

            mMainViewModel.ShutdownGdb();
            mMainViewModel.ExecuteGdb();

            mMainViewModel.CurrentInstruction = EDebugInstructionState.STANDBY;
        }
    }
}
