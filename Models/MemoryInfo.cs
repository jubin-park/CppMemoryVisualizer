using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CppMemoryVisualizer.Models
{
    class MemoryInfo
    {
        private uint mAddress;
        public uint Address
        {
            get { return mAddress; }
            set { mAddress = value; }
        }

        private byte[] mByteValues;
        public byte[] ByteValues
        {
            get { return mByteValues; }
            set { mByteValues = value; }
        }

        private uint mSize;
        public uint Size
        {
            get { return mSize; }
            set { mSize = value; }
        }

        private string mType = string.Empty;
        public string Type
        {
            get { return mType; }
            set { mType = value; }
        }

        private bool mbChanged;
        public bool IsChanged
        {
            get
            {
                return mbChanged;
            }
        }

        public void SetValue(string wordPattern)
        {
            mbChanged = false;

            uint count = 0;
            for (int i = 9; i < wordPattern.Length && count < mSize; i += 9)
            {
                for (int j = 0; j < 4; ++j)
                {
                    char c = wordPattern[i + j * 2 + 1];
                    if (c >= '0' && c <= '9')
                    {
                        c -= '0';
                    }
                    else if (c >= 'a' && c <= 'f')
                    {
                        c -= 'a';
                        c += (char)10;
                    }
                    else if (c >= 'A' && c <= 'F')
                    {
                        c -= 'A';
                        c += (char)10;
                    }
                    else
                    {
                        Debug.Assert(false);
                    }

                    char d = wordPattern[i + j * 2 + 2];
                    if (d >= '0' && d <= '9')
                    {
                        d -= '0';
                    }
                    else if (d >= 'a' && d <= 'f')
                    {
                        d -= 'a';
                        d += (char)10;
                    }
                    else if (d >= 'A' && d <= 'F')
                    {
                        d -= 'A';
                        d += (char)10;
                    }
                    else
                    {
                        Debug.Assert(false);
                    }

                    byte val = (byte)((byte)(16 * c) + (byte)d);
                    if (mByteValues[count] != val)
                    {
                        mbChanged = true;
                    }
                    mByteValues[count++] = val;
                }
            }
        }
    }
}
