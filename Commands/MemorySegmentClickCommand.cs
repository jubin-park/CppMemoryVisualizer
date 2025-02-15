﻿using CppMemoryVisualizer.Models;
using CppMemoryVisualizer.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace CppMemoryVisualizer.Commands
{
    abstract class MemorySegmentClickCommand : ICommand
    {
        public MainViewModel MainViewModel { get; }

        public MemorySegmentClickCommand(MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public virtual void Execute(object parameter)
        {
            Debug.Assert(parameter is uint);
            uint targetAddress = (uint)parameter;

            MainViewModel.ClearCapturedCounts();

            var stack = new Stack<MemorySegmentViewModel>();

            // CallStack
            foreach (var frame in MainViewModel.CallStackOrNull.StackFrames)
            {
                foreach (var local in frame.LocalVariables)
                {
                    foreach (var memorySegment in local.MemorySegments)
                    {
                        stack.Push(memorySegment);
                    }
                }
            }
            while (stack.Count > 0)
            {
                var pop = stack.Pop();
                pop.CapturedAddress = Enums.EMemoryArea.UNKNOWN;
                pop.CapturedValue = Enums.EMemoryArea.UNKNOWN;

                if (pop.Address == targetAddress)
                {
                    pop.CapturedAddress = Enums.EMemoryArea.CALL_STACK;
                    ++MainViewModel.CapturedStackMemoryAddressCount;
                }
                if (pop.Memory.Count == TypeInfo.POINTER_SIZE && null != pop.TypeName && pop.TypeName.Contains('*'))
                {
                    uint pointer = (uint)pop.Memory.Array[pop.Memory.Offset] |
                        (uint)pop.Memory.Array[pop.Memory.Offset + 1] << 8 |
                        (uint)pop.Memory.Array[pop.Memory.Offset + 2] << 16 |
                        (uint)pop.Memory.Array[pop.Memory.Offset + 3] << 24;
                    if (pointer == targetAddress)
                    {
                        pop.CapturedValue = MainViewModel.HeapManagerOrNull.GetHeapOrNull(pointer) != null ? Enums.EMemoryArea.HEAP : Enums.EMemoryArea.CALL_STACK;
                        ++MainViewModel.CapturedStackMemoryPointerValueCount;
                    }
                }

                foreach (var memberArray in pop.Children)
                {
                    foreach (var member in memberArray)
                    {
                        stack.Push(member);
                    }
                }
            }

            // Heap
            foreach (var heap in MainViewModel.HeapManagerOrNull.Heaps)
            {
                if (heap.MemorySegments != null)
                {
                    foreach (var memorySegment in heap.MemorySegments)
                    {
                        stack.Push(memorySegment);
                    }
                }
            }
            while (stack.Count > 0)
            {
                var pop = stack.Pop();
                pop.CapturedAddress = Enums.EMemoryArea.UNKNOWN;
                pop.CapturedValue = Enums.EMemoryArea.UNKNOWN;

                if (pop.Address == targetAddress)
                {
                    pop.CapturedAddress = Enums.EMemoryArea.HEAP;
                    ++MainViewModel.CapturedHeapMemoryAddressCount;
                }
                if (pop.Memory.Count == TypeInfo.POINTER_SIZE && null != pop.TypeName && pop.TypeName.Contains('*'))
                {
                    uint pointer = (uint)pop.Memory.Array[pop.Memory.Offset] |
                        (uint)pop.Memory.Array[pop.Memory.Offset + 1] << 8 |
                        (uint)pop.Memory.Array[pop.Memory.Offset + 2] << 16 |
                        (uint)pop.Memory.Array[pop.Memory.Offset + 3] << 24;
                    if (pointer == targetAddress)
                    {
                        pop.CapturedValue = MainViewModel.HeapManagerOrNull.GetHeapOrNull(pointer) != null ? Enums.EMemoryArea.HEAP : Enums.EMemoryArea.CALL_STACK;
                        ++MainViewModel.CapturedHeapMemoryPointerValueCount;
                    }
                }

                foreach (var memberArray in pop.Children)
                {
                    foreach (var member in memberArray)
                    {
                        stack.Push(member);
                    }
                }
            }
        }

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
    }
}
