using System.Diagnostics;
using System.Text;

namespace CppMemoryVisualizer.Models
{
    class MemoryOwnerInfo : MemoryInfo
    {
        protected byte[] mByteValues;
        public byte[] ByteValues
        {
            get
            {
                return mByteValues;
            }
            set
            {
                mByteValues = value;
            }
        }

        public void SetValue(StringBuilder wordPattern)
        {
            Debug.Assert(wordPattern != null);

            bool isChanged = false;

            uint count = 0;
            for (int i = 0; i < wordPattern.Length; i += 11)
            {
                for (int j = 3; j >= 0; --j)
                {
                    char c = wordPattern[i + j * 2 + 3];
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

                    char d = wordPattern[i + j * 2 + 4];
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
