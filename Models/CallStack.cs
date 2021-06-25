using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace CppMemoryVisualizer.Models
{
    sealed class CallStack : INotifyPropertyChanged
    {
        private readonly List<ulong> mStackFrameKeys = new List<ulong>(); // (ulong)stackFrameAddress << 32 | functionAddress;
        public List<ulong> StackFrameKeys
        {
            get
            {
                return mStackFrameKeys;
            }
        }

        private Dictionary<ulong, StackFrame> mStackFrameCaches = new Dictionary<ulong, StackFrame>();

        // https://stackoverflow.com/questions/21976979/mvvm-model-with-collections-use-or-not-observablecollection-in-model
        private ObservableCollection<StackFrame> mStackFrames = new ObservableCollection<StackFrame>();
        public ObservableCollection<StackFrame> StackFrames
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

        public void Push(uint stackFrameAddress, uint functionAddress, string functionName)
        {
            Debug.Assert(stackFrameAddress > 0);
            Debug.Assert(functionAddress > 0);
            Debug.Assert(functionName != null);

            ulong key = (ulong)stackFrameAddress << 32 | functionAddress;
            mStackFrameKeys.Add(key);

            StackFrame frame = null;
            if (!mStackFrameCaches.ContainsKey(key))
            {
                frame = new StackFrame(stackFrameAddress, functionAddress, functionName);
                mStackFrameCaches.Add(key, frame);
            }
            else
            {
                bool bSuccess = mStackFrameCaches.TryGetValue(key, out frame);
                Debug.Assert(bSuccess);
            }

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
            return 0 == mStackFrameKeys.Count;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
