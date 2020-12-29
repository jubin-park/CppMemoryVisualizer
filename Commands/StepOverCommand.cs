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
    class StepOverCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        private readonly MainViewModel mMainViewModel;

        public StepOverCommand(MainViewModel mainViewModel)
        {
            mMainViewModel = mainViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return mMainViewModel.ThreadCdbOrNull != null;
        }

        public void Execute(object parameter)
        {
            mMainViewModel.LastInstruction = EDebugInstructionState.STEP_OVER;
            mMainViewModel.SendInstruction(CdbInstructionSet.STEP_OVER);
            mMainViewModel.SendInstruction(CdbInstructionSet.DISPLAY_STACK_BACKTRACE);
            mMainViewModel.SendInstruction(CdbInstructionSet.DISPLAY_LOCAL_VARIABLE);
        }
    }
}
