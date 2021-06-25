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
using System.Windows.Input;
using System.Text.RegularExpressions;

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
            return mMainViewModel.ProcessGdbOrNull != null && EDebugInstructionState.STANDBY == mMainViewModel.CurrentInstruction;
        }

        public void Execute(object parameter)
        {
            mMainViewModel.CurrentInstruction = EDebugInstructionState.STEP_IN;

            bool isRangeIn = true;

            mMainViewModel.RequestInstruction(GdbInstructionSet.STEP_IN,
                GdbInstructionSet.REQUEST_START_STEP_IN_COMMAND, GdbInstructionSet.REQUEST_END_STEP_IN_COMMAND);
            mMainViewModel.ReadResultLine(GdbInstructionSet.REQUEST_START_STEP_IN_COMMAND, GdbInstructionSet.REQUEST_END_STEP_IN_COMMAND, (string line) =>
            {
                {
                    Regex rx = new Regex(@"at (.*):\d+$");
                    Match match = rx.Match(line);

                    if (match.Success && match.Groups[1].Value != Path.GetFileName(mMainViewModel.SourcePathOrNull))
                    {
                        isRangeIn = false;
                        return;
                    }
                }
                {
                    Regex rx = new Regex(@"^(\d+)\t(.*)");
                    Match match = rx.Match(line);

                    if (match.Success)
                    {
                        uint lineNumber = 0;
                        bool bSuccess = uint.TryParse(match.Groups[1].Value, out lineNumber);
                        Debug.Assert(bSuccess);
                        Debug.Assert(lineNumber > 0);
                        mMainViewModel.LinePointer = lineNumber;
                    }
                }
            });

            if (!isRangeIn)
            {
                mMainViewModel.RequestInstruction(GdbInstructionSet.FINISH,
                    GdbInstructionSet.REQUEST_START_FINISH_COMMAND, null);
                mMainViewModel.RequestInstruction(GdbInstructionSet.STEP_OVER,
                    null, GdbInstructionSet.REQUEST_END_FINISH_COMMAND);
                mMainViewModel.ReadResultLine(GdbInstructionSet.REQUEST_START_FINISH_COMMAND, GdbInstructionSet.REQUEST_END_FINISH_COMMAND,
                    mMainViewModel.ActionLinePointer);
            }

            mMainViewModel.UpdateGdb();

            mMainViewModel.CurrentInstruction = EDebugInstructionState.STANDBY;
        }
    }
}
