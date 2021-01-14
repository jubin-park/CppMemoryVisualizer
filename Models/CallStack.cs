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
        private ObservableCollection<StackFrame> mObservableStackFrames = new ObservableCollection<StackFrame>();
        public ObservableCollection<StackFrame> ObservableStackFrames
        {
            get
            {
                return mObservableStackFrames;
            }
        }

        private List<uint> mFunctionAddressList = new List<uint>();
        private Dictionary<uint, StackFrame> mStackFrames = new Dictionary<uint, StackFrame>();

        public void Clear()
        {
            mFunctionAddressList.Clear();
            App.Current.Dispatcher.Invoke(() =>
            {
                mObservableStackFrames.Clear();
            });       
        }

        public void Push(uint functionAddress, string functionName)
        {
            Debug.Assert(functionAddress > 0);
            Debug.Assert(functionName != null);

            if (!mStackFrames.ContainsKey(functionAddress))
            {
                mStackFrames.Add(functionAddress, new StackFrame(functionAddress, functionName));
            }
            mFunctionAddressList.Add(functionAddress);

            StackFrame frame = null;
            Debug.Assert(mStackFrames.TryGetValue(functionAddress, out frame));
            App.Current.Dispatcher.Invoke(() =>
            {
                mObservableStackFrames.Add(frame);
            });
            OnPropertyChanged("ObservableStackFrames");
        }

        public uint Top()
        {
            Debug.Assert(mFunctionAddressList.Count > 0);

            return mFunctionAddressList[0];
        }

        public StackFrame GetStackFrame(uint functionAddress)
        {
            Debug.Assert(functionAddress > 0);

            StackFrame stackFrame = null;
            Debug.Assert(mStackFrames.TryGetValue(functionAddress, out stackFrame));

            return stackFrame;
        }

        public bool IsEmpty()
        {
            return mFunctionAddressList.Count == 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
