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
            return mMainViewModel.ProcessGdbOrNull != null && mMainViewModel.CurrentInstruction == EDebugInstructionState.STANDBY;
        }

        public void Execute(object parameter)
        {
            mMainViewModel.CurrentInstruction = EDebugInstructionState.GO;

            mMainViewModel.RequestInstruction(GdbInstructionSet.GO,
                GdbInstructionSet.REQUEST_START_GO_COMMAND, GdbInstructionSet.REQUEST_END_GO_COMMAND);
            mMainViewModel.ReadResultLine(GdbInstructionSet.REQUEST_START_GO_COMMAND, GdbInstructionSet.REQUEST_END_GO_COMMAND,
                mMainViewModel.ActionLinePointer);

            /*
            var thread = new Thread(() =>
            {
                lock (mMainViewModel.LockObject)
                {
                    mMainViewModel.Update();
                    mMainViewModel.CurrentInstruction = EDebugInstructionState.STANDBY;
                }
            });

            thread.IsBackground = true;
            thread.Start();
            thread.Join();
            */

            mMainViewModel.CurrentInstruction = EDebugInstructionState.STANDBY;
        }
    }
}
