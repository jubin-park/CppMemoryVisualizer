﻿using CppMemoryVisualizer.Constants;
using CppMemoryVisualizer.Enums;
using CppMemoryVisualizer.ViewModels;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;

namespace CppMemoryVisualizer.Commands
{
    sealed class StepInCommand : ICommand
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

        public StepInCommand(MainViewModel mainViewModel)
        {
            mMainViewModel = mainViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return mMainViewModel.ProcessCdbOrNull != null && mMainViewModel.CurrentInstruction == EDebugInstructionState.STANDBY;
        }

        public void Execute(object parameter)
        {
            mMainViewModel.CurrentInstruction = EDebugInstructionState.STEP_IN;

            mMainViewModel.RequestInstruction(CdbInstructionSet.STEP_IN,
                CdbInstructionSet.REQUEST_START_STEP_IN_COMMAND, CdbInstructionSet.REQUEST_END_STEP_IN_COMMAND);
            mMainViewModel.ReadResultLine(CdbInstructionSet.REQUEST_START_STEP_IN_COMMAND, CdbInstructionSet.REQUEST_END_STEP_IN_COMMAND,
                mMainViewModel.ActionLinePointer);

            var thread = new Thread(() =>
            {
                lock (mMainViewModel.LockObject)
                {
                    bool loopGoUp = true;
                    while (loopGoUp)
                    {
                        mMainViewModel.RequestInstruction(CdbInstructionSet.DISPLAY_STACK_BACKTRACE + " 1",
                            CdbInstructionSet.REQUEST_START_GET_CALL_STACK, CdbInstructionSet.REQUEST_END_GET_CALL_STACK);
                        mMainViewModel.ReadResultLine(CdbInstructionSet.REQUEST_START_GET_CALL_STACK, CdbInstructionSet.REQUEST_END_GET_CALL_STACK, (string line) =>
                        {
                            if (line.Contains(mMainViewModel.SourcePathOrNull))
                            {
                                loopGoUp = false;
                            }
                        });

                        if (loopGoUp)
                        {
                            mMainViewModel.RequestInstruction(CdbInstructionSet.GO_UP + " 1",
                                null, null);
                        }
                    }

                    mMainViewModel.Update();
                    mMainViewModel.CurrentInstruction = EDebugInstructionState.STANDBY;
                }
            });

            thread.IsBackground = true;
            thread.Start();
            thread.Join(3000);
        }
    }
}
