using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;

namespace CppMemoryVisualizer.Models
{
    sealed class BreakPointList
    {
        private BitArray mIndices;
        public BitArray Indices
        {
            get
            {
                return mIndices;
            }
        }

        private uint mCount;
        public uint Count
        {
            get
            {
                return mCount;
            }
            set
            {
                mCount = value;
            }
        }

        private uint mLinePointer = 0;
        public uint LinePointer
        {
            get
            {
                return mLinePointer;
            }
        }

        public BreakPointList(uint capacity)
        {
            Debug.Assert(capacity > 0);

            mIndices = new BitArray((int)capacity);
        }
    }
}
