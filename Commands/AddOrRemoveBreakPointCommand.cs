using CppMemoryVisualizer.Constants;
using CppMemoryVisualizer.Models;
using CppMemoryVisualizer.ViewModels;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CppMemoryVisualizer.Commands
{
    sealed class AddOrRemoveBreakPointCommand : ICommand
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

        public AddOrRemoveBreakPointCommand(MainViewModel mainViewModel)
        {
            mMainViewModel = mainViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return mMainViewModel.ProcessGdbOrNull != null && mMainViewModel.CurrentInstruction == EDebugInstructionState.STANDBY;
        }

        public void Execute(object parameter)
        {
            string fileName = Path.GetFileName(mMainViewModel.SourcePathOrNull);
            uint selectedLineNumber = 0;

            if (parameter is string)
            {
                bool bSuccess = uint.TryParse((string)parameter, out selectedLineNumber);
                Debug.Assert(bSuccess);
            }
            else if (parameter is uint)
            {
                selectedLineNumber = (uint)parameter;
            }

            var breakpoints = mMainViewModel.BreakPointListOrNull.Indices;

            Debug.Assert(selectedLineNumber > 0u);

            if (!breakpoints[(int)selectedLineNumber])
            {
                mMainViewModel.CurrentInstruction = EDebugInstructionState.ADD_BREAK_POINT;
                mMainViewModel.RequestInstruction(string.Format(GdbInstructionSet.ADD_BREAK_POINT, fileName, selectedLineNumber),
                    GdbInstructionSet.REQUEST_START_ADD_BREAK_POINT, GdbInstructionSet.REQUEST_END_ADD_BREAK_POINT);
                mMainViewModel.ReadResultLine(GdbInstructionSet.REQUEST_START_ADD_BREAK_POINT, GdbInstructionSet.REQUEST_END_ADD_BREAK_POINT, (string line) =>
                {
                    Regex rx = new Regex(@"line (\d+).$");
                    Match match = rx.Match(line);

                    if (match.Success)
                    {
                        uint realLineNumber = 0;
                        bool bSuccess = uint.TryParse(match.Groups[1].Value, out realLineNumber);
                        Debug.Assert(bSuccess);
                        Debug.Assert(realLineNumber > 0);

                        ++mMainViewModel.BreakPointListOrNull.Count;
                        breakpoints[(int)realLineNumber] = true;
                    }
                });
            }
            else
            {
                mMainViewModel.CurrentInstruction = EDebugInstructionState.REMOVE_BREAK_POINT;
                mMainViewModel.RequestInstruction(string.Format(GdbInstructionSet.REMOVE_BREAK_POINT, fileName, selectedLineNumber),
                    null, null);
                breakpoints[(int)selectedLineNumber] = false;
            }

            mMainViewModel.CurrentInstruction = EDebugInstructionState.STANDBY;
        }
    }
}
