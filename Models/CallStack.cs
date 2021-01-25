using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Models
{
    sealed class CallStack : INotifyPropertyChanged
    {
        private readonly List<ulong> mKeys = new List<ulong>();
        private Dictionary<ulong, StackFrame> mStackFrames = new Dictionary<ulong, StackFrame>();

        // https://stackoverflow.com/questions/21976979/mvvm-model-with-collections-use-or-not-observablecollection-in-model
        private List<StackFrame> mStack = new List<StackFrame>(8);
        public List<StackFrame> Stack
        {
            get
            {
                return mStack;
            }
            set
            {
                mStack = value;
                onPropertyChanged("Stack");
            }
        }

        public void Clear()
        {
            mKeys.Clear();
            Stack = new List<StackFrame>(mStack.Count);
        }

        public void Push(uint stackAddress, uint functionAddress, string functionName)
        {
            Debug.Assert(stackAddress > 0);
            Debug.Assert(functionAddress > 0);
            Debug.Assert(functionName != null);

            ulong key = (ulong)stackAddress << 32 | functionAddress;
            mKeys.Add(key);

            StackFrame frame = null;
            if (!mStackFrames.ContainsKey(key))
            {
                frame = new StackFrame(stackAddress, functionAddress, functionName);
                mStackFrames.Add(key, frame);
            }
            else
            {
                Debug.Assert(mStackFrames.TryGetValue(key, out frame));
            }
            frame.Index = (uint)mStack.Count;
            frame.Y = frame.Index * 50;

            mStack.Add(frame);
        }

        public ulong Top()
        {
            Debug.Assert(mKeys.Count > 0);

            return mKeys[0];
        }

        public StackFrame GetStackFrame(ulong key)
        {
            Debug.Assert(key > 0);

            StackFrame stackFrame = null;
            Debug.Assert(mStackFrames.TryGetValue(key, out stackFrame));

            return stackFrame;
        }

        public bool IsEmpty()
        {
            return mKeys.Count == 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
