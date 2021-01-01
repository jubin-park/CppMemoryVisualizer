using CppMemoryVisualizer.ViewModels;
using CppMemoryVisualizer.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CppMemoryVisualizer.Commands
{
    class AddOrRemoveBreakPointCommand : ICommand
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
            return mMainViewModel.ThreadCdbOrNull != null;
        }

        public void Execute(object parameter)
        {
            string fileName = Path.GetFileName(mMainViewModel.SourcePathOrNull);
            uint line = 0;

            if (parameter is string)
            {
                Debug.Assert(uint.TryParse((string)parameter, out line));
            }
            else if (parameter is uint)
            {
                line = (uint)parameter;
            }

            Debug.Assert(line > 0u);

            uint breakpointIndex = mMainViewModel.BreakPointInfoOrNull.Indices[line];
            mMainViewModel.BreakPointInfoOrNull.Clear();

            if (breakpointIndex == uint.MaxValue)
            {
                mMainViewModel.CurrentInstruction = EDebugInstructionState.ADD_BREAK_POINT;
                mMainViewModel.SendInstruction(string.Format(CdbInstructionSet.SET_BREAK_POINT_SOURCE_LEVEL, fileName, line));
            }
            else
            {
                mMainViewModel.CurrentInstruction = EDebugInstructionState.REMOVE_BREAK_POINT;
                mMainViewModel.SendInstruction(string.Format(CdbInstructionSet.CLEAR_BREAK_POINT, breakpointIndex));
            }

            mMainViewModel.SendInstruction(CdbInstructionSet.DISPLAY_BREAK_POINT_LIST);
        }
    }
}
