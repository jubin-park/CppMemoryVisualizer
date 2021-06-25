using CppMemoryVisualizer.Constants;
using CppMemoryVisualizer.ViewModels;
using System;
using System.Windows.Input;

namespace CppMemoryVisualizer.Commands
{
    sealed class GoCommand : ICommand
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

        public GoCommand(MainViewModel mainViewModel)
        {
            mMainViewModel = mainViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return mMainViewModel.ProcessGdbOrNull != null && EDebugInstructionState.STANDBY == mMainViewModel.CurrentInstruction;
        }

        public void Execute(object parameter)
        {
            mMainViewModel.CurrentInstruction = EDebugInstructionState.GO;

            mMainViewModel.RequestInstruction(GdbInstructionSet.GO,
                GdbInstructionSet.REQUEST_START_GO_COMMAND, GdbInstructionSet.REQUEST_END_GO_COMMAND);
            mMainViewModel.ReadResultLine(GdbInstructionSet.REQUEST_START_GO_COMMAND, GdbInstructionSet.REQUEST_END_GO_COMMAND,
                mMainViewModel.ActionLinePointer);

            mMainViewModel.UpdateGdb();
            mMainViewModel.CurrentInstruction = EDebugInstructionState.STANDBY;
        }
    }
}
