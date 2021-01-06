using CppMemoryVisualizer.Constants;
using CppMemoryVisualizer.Enums;
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
            return mMainViewModel.ProcessCdbOrNull != null && mMainViewModel.CurrentInstruction == EDebugInstructionState.STANDBY;
        }

        public void Execute(object parameter)
        {
            mMainViewModel.CurrentInstruction = EDebugInstructionState.GO;

            mMainViewModel.RequestInstruction(CdbInstructionSet.GO,
                CdbInstructionSet.REQUEST_START_GO_COMMAND, CdbInstructionSet.REQUEST_END_GO_COMMAND);
            mMainViewModel.ReadResultLine(CdbInstructionSet.REQUEST_START_GO_COMMAND, CdbInstructionSet.REQUEST_END_GO_COMMAND,
                mMainViewModel.ActionLinePointer);

            var thread = new Thread(() =>
            {
                mMainViewModel.Update();
                mMainViewModel.CurrentInstruction = EDebugInstructionState.STANDBY;
            });

            thread.Start();
            thread.Join(3000);
        }
    }
}
