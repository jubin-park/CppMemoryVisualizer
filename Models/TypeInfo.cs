using CppMemoryVisualizer.Constants;
using CppMemoryVisualizer.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CppMemoryVisualizer.Models
{
    sealed class TypeInfo
    {
        public static readonly uint POINTER_SIZE = 4;

        public TypeInfo()
        {

        }

        public TypeInfo(TypeInfo other) // recursive copy constructor
        {
            mFullNameOrNull = other.mFullNameOrNull;
            mPureName = other.mPureName;
            mSize = other.mSize;
            mFlags = other.mFlags;
            mPointerLevel = other.mPointerLevel;

            mArrayOrFunctionPointerLevels = new List<uint>();
            foreach (uint level in other.mArrayOrFunctionPointerLevels)
            {
                mArrayOrFunctionPointerLevels.Add(level);
            }

            mArrayLengths = new List<uint>();
            foreach (uint length in other.mArrayLengths)
            {
                mArrayLengths.Add(length);
            }

            mOffset = other.mOffset;
            mMemberNameOrNull = other.mMemberNameOrNull;

            mMembers = new List<TypeInfo>();
            foreach (var member in other.mMembers)
            {
                mMembers.Add(new TypeInfo(member));
            }
        }

        private string mFullNameOrNull;
        public string FullNameOrNull
        {
            get
            {
                return mFullNameOrNull;
            }
        }

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

        public uint GetTotalLength()
        {
            uint totalLength = 1;

            if (0 == mArrayOrFunctionPointerLevels.Count)
            {
                foreach (uint len in mArrayLengths)
                {
                    totalLength *= len;
                }
            }

            return totalLength;
        }

        public void SetByString(string typeName)
        {
            Debug.Assert(null != typeName);

            int templateAngleBracketPairCount = 0;
            int templateTypeLength = 0;

            for (int i = 0; i < typeName.Length; ++i)
            {
                char c = typeName[i];
                if ('<' == c)
                {
                    ++templateAngleBracketPairCount;
                }
                else if ('>' == c && --templateAngleBracketPairCount == 0)
                {
                    templateTypeLength = i + 1;
                    break;
                }
            }

            mFullNameOrNull = typeName;

            string templateNameOrNull = null;
            if (templateTypeLength > 0)
            {
                templateNameOrNull = typeName.Substring(0, templateTypeLength);
                typeName = "T" + typeName.Substring(templateTypeLength);
            }

            Match match = RegexSet.REGEX_ONE_LINE_TYPE.Match(typeName);
            Debug.Assert(match.Success);

            if (templateNameOrNull is null)
            {
                mPureName = match.Groups[1].Value.Trim();
            }
            else
            {
                mPureName = templateNameOrNull + typeName.Substring(1);
            }

            string pointerChars = match.Groups[3].Value;
            string arrayOrFunctionPointerChars = match.Groups[4].Value;
            string dimensions = match.Groups[5].Value;
            string reference = match.Groups[6].Value;

            if (mFullNameOrNull.Contains("std::"))
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

        public TypeInfo GetDereference()
        {
            Debug.Assert(mPointerLevel > 0);

            TypeInfo dereferenceType = new TypeInfo();

            dereferenceType.mFlags = mFlags;
            //dereferenceType.mMemberNameOrNull = mMemberNameOrNull;
            dereferenceType.mPointerLevel = (uint)Math.Max((int)mPointerLevel - 1, 0);

            if (dereferenceType.mPointerLevel > 0)
            {
                dereferenceType.mFlags &= ~(EMemoryTypeFlags.POINTER | EMemoryTypeFlags.ARRAY);
                dereferenceType.SetByString(mPureName + ' ' + new string('*', (int)dereferenceType.mPointerLevel));
                dereferenceType.mSize = TypeInfo.POINTER_SIZE;
            }
            else
            {
                dereferenceType.SetByString(mPureName);
                dereferenceType.mMembers = PureTypeManager.GetType(mPureName).Members;
                dereferenceType.mSize = PureTypeManager.GetType(mPureName).mSize;
            }

            return dereferenceType;
        }

        public TypeInfo GetElementOfArray()
        {
            //Debug.Assert(mArrayLengths.Count > 0);

            TypeInfo elementType = new TypeInfo();

            elementType.mFlags = mFlags & ~(EMemoryTypeFlags.ARRAY);
            elementType.mPointerLevel = mPointerLevel;
            elementType.mMemberNameOrNull = mMemberNameOrNull;
            elementType.mMembers = mMembers;

            uint length = GetTotalLength();
            elementType.mSize = mSize / length;            

            if (null != mPureName)
            {
                string name = mPureName;
                if (mPointerLevel > 0)
                {
                    name += ' ';
                    name += new string('*', (int)mPointerLevel);
                }

                elementType.SetByString(name);
            }

            return elementType;
        }
    }
}
