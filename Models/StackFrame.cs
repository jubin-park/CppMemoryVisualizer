using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Models
{
    sealed class StackFrame : INotifyPropertyChanged
    {
        public bool IsInitialized = false;

        private readonly List<string> mLocalVariableNames = new List<string>();
        public List<string> LocalVariableNames
        {
            get
            {
                return mLocalVariableNames;
            }
        }

        private Dictionary<string, LocalVariable> mLocalVariableCaches = new Dictionary<string, LocalVariable>();
        public Dictionary<string, LocalVariable> LocalVariableCaches
        {
            get
            {
                return mLocalVariableCaches;
            }
        }

        private List<LocalVariable> mLocalVariables = new List<LocalVariable>();
        public List<LocalVariable> LocalVariables
        {
            get
            {
                return mLocalVariables;
            }
            set
            {
                mLocalVariables = value;
                onPropertyChanged(nameof(mLocalVariables));
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

        private double mX;
        public double X
        {
            get
            {
                return mX;
            }
            set
            {
                mX = value;
                onPropertyChanged("X");
            }
        }

        private double mY;
        public double Y
        {
            get
            {
                return mY;
            }
            set
            {
                mY = value;
                onPropertyChanged("Y");
            }
        }

        public StackFrame(uint stackAddress, uint functionAddress, string functionName)
        {
            Debug.Assert(stackAddress > 0);
            Debug.Assert(functionAddress > 0);
            Debug.Assert(functionName != null);

            mStackAddress = stackAddress;
            mFunctionAddress = functionAddress;
            mName = functionName;
        }

        public bool TryAdd(string localVariableName, bool isArgument)
        {
            Debug.Assert(localVariableName != null);

            if (!mLocalVariableCaches.ContainsKey(localVariableName))
            {
                mLocalVariableNames.Add(localVariableName);

                var local = new LocalVariable(localVariableName, isArgument);
                mLocalVariableCaches.Add(localVariableName, local);
                mLocalVariables.Add(local);

                return true;
            }

            return false;
        }

        public LocalVariable GetLocalVariable(string localVariableName)
        {
            Debug.Assert(localVariableName != null);

            LocalVariable localVariable = null;
            mLocalVariableCaches.TryGetValue(localVariableName, out localVariable);

            return localVariable;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
