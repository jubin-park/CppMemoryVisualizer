using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Models
{
    class CallStack
    {
        private List<uint> mFunctionAddressList = new List<uint>();
        private Dictionary<uint, StackFrame> mStackFrames = new Dictionary<uint, StackFrame>();

        public void Clear()
        {
            mFunctionAddressList.Clear();
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
        }

        public uint Top()
        {
            Debug.Assert(mFunctionAddressList.Count > 0);

            return mFunctionAddressList[0];
        }

        public StackFrame GetStackFrame(uint functionAddress)
        {
            Debug.Assert(functionAddress > 0);

            StackFrame stackFrame;
            Debug.Assert(mStackFrames.TryGetValue(functionAddress, out stackFrame));

            return stackFrame;
        }

        public bool IsEmpty()
        {
            return mFunctionAddressList.Count == 0;
        }
    }
}
