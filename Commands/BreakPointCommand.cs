using CppMemoryVisualizer.ViewModels;
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
    class BreakPointCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        private readonly MainViewModel mMainViewModel;

        public BreakPointCommand(MainViewModel mainViewModel)
        {
            mMainViewModel = mainViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return mMainViewModel.ProcessCdbOrNull != null && mMainViewModel.ThreadCdbOrNull != null;
        }

        public void Execute(object parameter)
        {
            string fileName = Path.GetFileName(mMainViewModel.SourcePathOrNull);
            uint line = 0;
            Debug.Assert(uint.TryParse((string)parameter, out line));

            Debug.Assert(line > 0);

            mMainViewModel.LastInstruction = EDebugInstructionState.BREAK_POINT;
            mMainViewModel.SendInstruction(string.Format(CdbInstructionSet.SET_BREAK_POINT_SOURCE_LEVEL, fileName, line));
        }
    }
}
