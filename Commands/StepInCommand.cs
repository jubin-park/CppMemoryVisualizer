﻿using CppMemoryVisualizer.ViewModels;
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
    class StepInCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly MainViewModel mMainViewModel;

        public StepInCommand(MainViewModel mainViewModel)
        {
            mMainViewModel = mainViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            mMainViewModel.Instruction = EDebugInstructionType.STEP_IN;
            mMainViewModel.SendInstruction("t");
        }
    }
}
