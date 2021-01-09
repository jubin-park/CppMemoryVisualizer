using CppMemoryVisualizer.Enums;
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

        private uint mUnitSize;
        public uint UnitSize
        {
            get { return mUnitSize; }
            set { mUnitSize = value; }
        }

        public uint Length
        {
            get { return mSize / mUnitSize; }
        }

        private string mTypeName = string.Empty;
        public string TypeName
        {
            get { return mTypeName; }
            set { mTypeName = value; }
        }

        private EMemoryTypeFlags mTypeFlags = 0;
        public EMemoryTypeFlags TypeFlags
        {
            get { return mTypeFlags; }
            set { mTypeFlags = value; }
        }

        private uint mPointerLevel;
        public uint PointerLevel
        {
            get
            {
                return mPointerLevel;
            }
            set
            {
                mPointerLevel = value;
            }
        }

        private readonly List<uint> mArrayOrFunctionPointerLevels = new List<uint>();
        public List<uint> ArrayOrFunctionPointerLevels
        {
            get
            {
                return mArrayOrFunctionPointerLevels;
            }
        }

        private readonly List<uint> mArrayLengths = new List<uint>();
        public List<uint> ArrayLengths
        {
            get
            {
                return mArrayLengths;
            }
        }

        private bool mbChanged;
        public bool IsChanged
        {
            get
            {
                return mbChanged;
            }
        }

        public static string GetFullTypeName(string typeName, uint pointerLevel, List<uint> arrayOrFunctionPointerLevels, List<uint> arrayLengths)
        {
            StringBuilder sb = new StringBuilder(typeName, 128);
            sb.Append(' ');

            if (pointerLevel > 0)
            {
                sb.Append('*', (int)pointerLevel);
            }
            foreach (uint len in arrayOrFunctionPointerLevels)
            {
                sb.Append('(');
                sb.Append(new string('*', (int)len));
                sb.Append(')');
            }
            foreach (uint len in arrayLengths)
            {
                sb.AppendFormat("[{0}]", len);
            }

            if (sb[sb.Length - 1] == ' ')
            {
                --sb.Length;
            }

            return sb.ToString();
        }

        public void SetValue(string wordPattern)
        {
            bool isChanged = false;

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
                        isChanged = true;
                    }
                    mByteValues[count++] = val;
                }
            }

            mbChanged = isChanged;
        }
    }
}
