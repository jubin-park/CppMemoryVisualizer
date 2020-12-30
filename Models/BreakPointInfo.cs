using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CppMemoryVisualizer.Models
{
    class BreakPointInfo
    {
        private uint[] mIndices;
        public uint[] Indices
        {
            get => mIndices;
        }

        public uint Count
        {
            get => mIndices[0];
            set
            {
                Debug.Assert(mIndices != null);

                mIndices[0] = value;
            }
        }

        private uint mOldCount;
        public uint OldCount
        {
            get => mOldCount;
        }

        private uint mLinePointer = 0;
        public uint LinePointer
        {
            get => mLinePointer;
        }

        public BreakPointInfo(uint capacity)
        {
            mIndices = new uint[capacity];
            Clear();
        }

        public void Clear()
        {
            mOldCount = Count;
            Count = 0;
            for (int i = 1; i < mIndices.Length; ++i)
            {
                mIndices[i] = uint.MaxValue;
            }
        }

        public bool IsUpdatable()
        {
            return OldCount == 0 || Count + 1 >= OldCount && Count <= OldCount + 1;
        }

        public void ProcessLine(string line)
        {
            string[] bpInfos = line.Trim().Split(' ');
            if (bpInfos[2] == "redefined")
            {
                return;
            }

            uint bpIndex = uint.MaxValue;
            Debug.Assert(uint.TryParse(bpInfos[0], out bpIndex));
            Debug.Assert(bpIndex < uint.MaxValue);

            uint lineNumber = 0;
            Debug.Assert(uint.TryParse(bpInfos[5].Remove(bpInfos[5].Length - 1), out lineNumber));
            Debug.Assert(lineNumber != 0);

            ++Count;
            mIndices[lineNumber] = bpIndex;
        }
    }
}
