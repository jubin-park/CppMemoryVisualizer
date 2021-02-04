using CppMemoryVisualizer.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Models
{
    sealed class TypeInfo
    {
        private static readonly Regex regex = new Regex(@"([a-zA-Z0-9_<>,: ]+)($|\s(\**)([\(\*+\)]*)([\[\d+\]]*)(&{0,1}))");

        private string mPureName;
        public string PureName
        {
            get
            {
                return mPureName;
            }
            set
            {
                if ("std::basic_string<char,std::char_traits<char>,std::allocator<char> >" == value)
                {
                    value = "std::string";
                }
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

        private EMemoryTypeFlags mFlags = EMemoryTypeFlags.NONE;
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

        private List<TypeInfo> mMembers = new List<TypeInfo>();
        public List<TypeInfo> Members
        {
            get
            {
                return mMembers;
            }
            set
            {
                mMembers = value;
            }
        }
        #endregion

        private string mFullNameOrNull;
        public string FullNameOrNull
        {
            get
            {
                return mFullNameOrNull;
            }
            set
            {
                mFullNameOrNull = value;

                Match match = regex.Match(value);

                if (match.Success)
                {
                    PureName = match.Groups[1].Value;
                    string pointerChars = match.Groups[3].Value;
                    string arrayOrFunctionPointerChars = match.Groups[4].Value;
                    string dimensions = match.Groups[5].Value;
                    string reference = match.Groups[6].Value;

                    if (PureName.StartsWith("std::"))
                    {
                        mFlags |= EMemoryTypeFlags.STL;
                    }

                    if (pointerChars.Length > 0)
                    {
                        mFlags |= EMemoryTypeFlags.POINTER;
                        mPointerLevel = (uint)pointerChars.Length;
                    }

                    if (arrayOrFunctionPointerChars.Length > 0)
                    {
                        mFlags |= EMemoryTypeFlags.ARRAY_OR_FUNCTION_POINTER;

                        Regex regex = new Regex(@"\((\*+)\)");
                        Match matchPointer = regex.Match(arrayOrFunctionPointerChars);

                        while (matchPointer.Success)
                        {
                            uint size = (uint)matchPointer.Groups[1].Length;
                            mArrayOrFunctionPointerLevels.Add(size);

                            matchPointer = matchPointer.NextMatch();
                        }
                    }

                    if (dimensions.Length > 0)
                    {
                        mFlags |= EMemoryTypeFlags.ARRAY;

                        Regex regex = new Regex(@"\[(\d+)\]");
                        Match matchDimension = regex.Match(dimensions);

                        while (matchDimension.Success)
                        {
                            uint size = 0;
                            bool bSuccess = uint.TryParse(matchDimension.Groups[1].Value, out size);
                            Debug.Assert(bSuccess);
                            mArrayLengths.Add(size);

                            matchDimension = matchDimension.NextMatch();
                        }
                    }

                    if (reference.Length > 0)
                    {
                        mFlags |= EMemoryTypeFlags.REFERENCE;
                    }
                }
            }
        }

        public TypeInfo GetDereferenceOrNull()
        {
            Debug.Assert(mFlags.HasFlag(EMemoryTypeFlags.POINTER) && mPointerLevel > 0);

            TypeInfo type = new TypeInfo();

            string name = mPureName;
            if (mPointerLevel > 0)
            {
                name += ' ';
                name += new string('*', (int)mPointerLevel - 1);
            }
            type.FullNameOrNull = name;

            return type;
        }
    }
}
