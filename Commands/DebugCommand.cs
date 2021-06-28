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
            return mMainViewModel.SourcePathOrNull != null && (EDebugInstructionState.STANDBY == mMainViewModel.CurrentInstruction || EDebugInstructionState.DEAD == mMainViewModel.CurrentInstruction);
        }

        public void Execute(object parameter)
        {
            Debug.Assert(mMainViewModel.SourcePathOrNull != null);

            mMainViewModel.CurrentInstruction = EDebugInstructionState.START_DEBUGGING;

            mMainViewModel.ShutdownGdb();
            if (!mMainViewModel.ExecuteGdb())
            {
                mMainViewModel.CurrentInstruction = EDebugInstructionState.DEAD;
            }
        }
    }
}
