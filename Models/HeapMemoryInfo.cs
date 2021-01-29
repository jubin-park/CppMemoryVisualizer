using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Models
{
    sealed class HeapMemoryInfo : MemoryOwnerInfo
    {
        private uint mSize;
        public uint Size
        {
            get
            {
                return mSize;
            }
        }

        private bool mbVisible;
        public bool IsVisible
        {
            get
            {
                return mbVisible;
            }
            set
            {
                mbVisible = value;
            }
        }

        public HeapMemoryInfo(uint address, uint size)
        {
            mAddress = address;
            mSize = size;

            uint wordCount = size / 4 + (size % 4 > 0 ? 1u : 0);
            mByteValues = new byte[wordCount * 4];
        }
    }
}
