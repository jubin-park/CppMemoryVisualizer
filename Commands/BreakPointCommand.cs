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
        public event EventHandler CanExecuteChanged;

        private readonly MainViewModel mMainViewModel;

        public BreakPointCommand(MainViewModel mainViewModel)
        {
            mMainViewModel = mainViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            string fileName = Path.GetFileName(mMainViewModel.SourcePathOrNull);
            uint line = 0;
            Debug.Assert(uint.TryParse((string)parameter, out line));

            mMainViewModel.Instruction = EDebugInstructionType.BREAK_POINT;
            mMainViewModel.SendInstruction($"bp (@@masm(`{fileName}:{line}+`))");
        }
    }
}
