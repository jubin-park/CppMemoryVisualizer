﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CppMemoryVisualizer.Models
{
    sealed class BreakPointList
    {
        private uint[] mIndices;
        public uint[] Indices
        {
            get
            {
                return mIndices;
            }
        }

        public uint Count
        {
            get
            {
                return mIndices[0];
            }
            set
            {
                Debug.Assert(mIndices != null);

                mIndices[0] = value;
            }
        }

        private uint mOldCount;
        public uint OldCount
        {
            get
            {
                return mOldCount;
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
            return Count + 1 >= OldCount && Count <= OldCount + 1;
        }
    }
}