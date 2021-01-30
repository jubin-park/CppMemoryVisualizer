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
        private readonly List<ulong> mStackFrameKeys = new List<ulong>();
        public List<ulong> StackFrameKeys
        {
            get
            {
                return mStackFrameKeys;
            }
        }

        private Dictionary<ulong, StackFrame> mStackFrameCaches = new Dictionary<ulong, StackFrame>();

        // https://stackoverflow.com/questions/21976979/mvvm-model-with-collections-use-or-not-observablecollection-in-model
        private List<StackFrame> mStackFrames = new List<StackFrame>();
        public List<StackFrame> StackFrames
        {
            get
            {
                return mStackFrames;
            }
            set
            {
                mStackFrames = value;
                onPropertyChanged("StackFrames");
            }
        }

        public void Clear()
        {
            mStackFrameKeys.Clear();
            StackFrames.Clear();
        }

        public void Push(uint stackAddress, uint functionAddress, string functionName)
        {
            Debug.Assert(stackAddress > 0);
            Debug.Assert(functionAddress > 0);
            Debug.Assert(functionName != null);

            ulong key = (ulong)stackAddress << 32 | functionAddress;
            mStackFrameKeys.Add(key);

            StackFrame frame = null;
            if (!mStackFrameCaches.ContainsKey(key))
            {
                frame = new StackFrame(stackAddress, functionAddress, functionName);
                mStackFrameCaches.Add(key, frame);
            }
            else
            {
                bool bSuccess = mStackFrameCaches.TryGetValue(key, out frame);
                Debug.Assert(bSuccess);
            }
            frame.Index = (uint)mStackFrames.Count;
            frame.Y = frame.Index * 50;

            mStackFrames.Add(frame);
        }

        public ulong Top()
        {
            Debug.Assert(mStackFrameKeys.Count > 0);

            return mStackFrameKeys[0];
        }

        public StackFrame GetStackFrame(ulong key)
        {
            Debug.Assert(key > 0);

            StackFrame stackFrame = null;
            bool bSuccess = mStackFrameCaches.TryGetValue(key, out stackFrame);
            Debug.Assert(bSuccess);

            return stackFrame;
        }

        public bool IsEmpty()
        {
            return mStackFrameKeys.Count == 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
