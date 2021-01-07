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
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        private readonly MainViewModel mMainViewModel;

        public AddOrRemoveBreakPointCommand(MainViewModel mainViewModel)
        {
            mMainViewModel = mainViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return mMainViewModel.ProcessCdbOrNull != null && mMainViewModel.CurrentInstruction == EDebugInstructionState.STANDBY;
        }

        public void Execute(object parameter)
        {
            string fileName = Path.GetFileName(mMainViewModel.SourcePathOrNull);
            uint SelectedLineNumber = 0;

            if (parameter is string)
            {
                Debug.Assert(uint.TryParse((string)parameter, out SelectedLineNumber));
            }
            else if (parameter is uint)
            {
                SelectedLineNumber = (uint)parameter;
            }

            Debug.Assert(SelectedLineNumber > 0u);

            uint breakpointIndex = mMainViewModel.BreakPointInfoOrNull.Indices[SelectedLineNumber];
            mMainViewModel.BreakPointInfoOrNull.Clear();

            if (breakpointIndex == uint.MaxValue)
            {
                mMainViewModel.RequestInstruction(string.Format(CdbInstructionSet.SET_BREAK_POINT_SOURCE_LEVEL, fileName, SelectedLineNumber),
                    null, null);
            }
            else
            {
                mMainViewModel.RequestInstruction(string.Format(CdbInstructionSet.CLEAR_BREAK_POINT, breakpointIndex),
                    null, null);
            }

            mMainViewModel.RequestInstruction(string.Format(CdbInstructionSet.DISPLAY_BREAK_POINT_LIST, fileName, SelectedLineNumber),
                CdbInstructionSet.REQUEST_START_DISPLAY_BREAK_POINT_LIST, CdbInstructionSet.REQUEST_END_DISPLAY_BREAK_POINT_LIST);
            mMainViewModel.ReadResultLine(CdbInstructionSet.REQUEST_START_DISPLAY_BREAK_POINT_LIST, CdbInstructionSet.REQUEST_END_DISPLAY_BREAK_POINT_LIST, (string line) =>
            {
                Regex rx = new Regex(@"^\s?(\d+)\se\s[0-9a-f]{8}\s\[(.+|:|\\)\s@\s(\d+)\]");
                Match match = rx.Match(line);

                if (match.Success)
                {
                    uint bpIndex = uint.MaxValue;
                    Debug.Assert(uint.TryParse(match.Groups[1].Value, out bpIndex));
                    Debug.Assert(bpIndex < uint.MaxValue);

                    uint realLineNumber = 0;
                    Debug.Assert(uint.TryParse(match.Groups[3].Value, out realLineNumber));
                    Debug.Assert(realLineNumber > 0);

                    ++mMainViewModel.BreakPointInfoOrNull.Count;
                    mMainViewModel.BreakPointInfoOrNull.Indices[realLineNumber] = bpIndex;
                }
            });
        }
    }
}
