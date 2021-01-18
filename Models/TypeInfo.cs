using CppMemoryVisualizer.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Models
{
    sealed class TypeInfo
    {
        private string mPureName;
        public string PureName
        {
            get
            {
                return mPureName;
            }
            set
            {
                mPureName = value;
            }
        }

        private uint mSize;
        public uint Size
        {
            get
            {
                return mSize;
            }
            set
            {
                mSize = value;
            }
        }

        private EMemoryTypeFlags mFlags = 0;
        public EMemoryTypeFlags Flags
        {
            get
            {
                return mFlags;
            }
            set
            {
                mFlags = value;
            }
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

        #region struct or class
        private uint mOffset;
        public uint Offset
        {
            get
            {
                return mOffset;
            }
            set
            {
                mOffset = value;
            }
        }

        private string mMemberNameOrNull;
        public string MemberNameOrNull
        {
            get
            {
                return mMemberNameOrNull;
            }
            set
            {
                mMemberNameOrNull = value;
            }
        }

        private readonly List<TypeInfo> mMembers = new List<TypeInfo>();
        public List<TypeInfo> Members
        {
            get
            {
                return mMembers;
            }
        }
        #endregion

        public string FullName
        {
            get
            {
                StringBuilder sb = new StringBuilder(mPureName, 128);
                sb.Append(' ');

                if (mPointerLevel > 0)
                {
                    sb.Append('*', (int)mPointerLevel);
                }
                foreach (uint len in mArrayOrFunctionPointerLevels)
                {
                    sb.Append('(');
                    sb.Append(new string('*', (int)len));
                    sb.Append(')');
                }
                foreach (uint len in mArrayLengths)
                {
                    sb.AppendFormat("[{0}]", len);
                }

                if (sb[sb.Length - 1] == ' ')
                {
                    --sb.Length;
                }

                return sb.ToString();
            }
        }
    }
}
