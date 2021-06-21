using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CppMemoryVisualizer.Models
{
    sealed class StackFrame
    {
        private readonly ObservableCollection<LocalVariable> mLocalVariables = new ObservableCollection<LocalVariable>();
        public ObservableCollection<LocalVariable> LocalVariables
        {
            get
            {
                return mLocalVariables;
            }
        }

        private string mName;
        public string Name
        {
            get
            {
                return mName;
            }
        }

        private uint mStackAddress;
        public uint StackAddress
        {
            get
            {
                return mStackAddress;
            }
        }

        private uint mFunctionAddress;
        public uint FunctionAddress
        {
            get
            {
                return mFunctionAddress;
            }
        }

        private uint mIndex;
        public uint Index
        {
            get
            {
                return mIndex;
            }
            set
            {
                mIndex = value;
            }
        }

        public StackFrame(uint stackAddress, uint functionAddress, string functionName)
        {
            Debug.Assert(stackAddress > 0);
            Debug.Assert(functionAddress > 0);
            Debug.Assert(null != functionName);

            mStackAddress = stackAddress;
            mFunctionAddress = functionAddress;
            mName = functionName;
        }

        public void Clear()
        {
            mLocalVariables.Clear();
        }

        public void AddArgumentLocalVariable(string argumentVariableName)
        {
            Debug.Assert(null != argumentVariableName);
            mLocalVariables.Add(new LocalVariable(argumentVariableName, true));
        }

        public void AddLocalVariable(string localVariableName)
        {
            Debug.Assert(null != localVariableName);
            mLocalVariables.Add(new LocalVariable(localVariableName, false));
        }
    }
}
