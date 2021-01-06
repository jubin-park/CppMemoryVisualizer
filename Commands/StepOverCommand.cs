﻿using CppMemoryVisualizer.Constants;
using CppMemoryVisualizer.Enums;
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
            return mMainViewModel.ProcessCdbOrNull != null && mMainViewModel.CurrentInstruction == EDebugInstructionState.STANDBY;
        }

        public void Execute(object parameter)
        {
            mMainViewModel.CurrentInstruction = EDebugInstructionState.STEP_OVER;

            mMainViewModel.RequestInstruction(CdbInstructionSet.STEP_OVER,
                CdbInstructionSet.REQUEST_START_STEP_OVER_COMMAND, CdbInstructionSet.REQUEST_END_STEP_OVER_COMMAND);
            mMainViewModel.ReadResultLine(CdbInstructionSet.REQUEST_START_STEP_OVER_COMMAND, CdbInstructionSet.REQUEST_END_STEP_OVER_COMMAND,
                mMainViewModel.ActionLinePointer);

            mMainViewModel.Update();

            mMainViewModel.CurrentInstruction = EDebugInstructionState.STANDBY;
        }
    }
}
