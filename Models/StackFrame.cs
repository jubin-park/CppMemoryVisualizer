using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Models
{
    sealed class StackFrame
    {
        public bool IsInitialized = false;

        private uint mAddress;
        private string mName;

        private readonly List<string> mLocalVariableNames = new List<string>();
        public List<string> LocalVariableNames
        {
            get
            {
                return mLocalVariableNames;
            }
        }

        private Dictionary<string, LocalVariable> mLocalVariables = new Dictionary<string, LocalVariable>();
        public Dictionary<string, LocalVariable> LocalVariables
        {
            get
            {
                return mLocalVariables;
            }
        }

        public StackFrame(uint address, string name)
        {
            Debug.Assert(address > 0);
            Debug.Assert(name != null);

            mAddress = address;
            mName = name;
        }

        public bool TryAdd(string localVariableName)
        {
            Debug.Assert(localVariableName != null);

            if (!mLocalVariables.ContainsKey(localVariableName))
            {
                mLocalVariables.Add(localVariableName, new LocalVariable());
                mLocalVariableNames.Add(localVariableName);

                return true;
            }

            return false;
        }

        public LocalVariable GetLocalVariable(string localVariableName)
        {
            Debug.Assert(localVariableName != null);

            LocalVariable localVariable = null;
            Debug.Assert(mLocalVariables.TryGetValue(localVariableName, out localVariable));

            return localVariable;
        }
    }
}
