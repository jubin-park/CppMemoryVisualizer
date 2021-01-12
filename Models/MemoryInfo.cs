using CppMemoryVisualizer.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Models
{
    class MemoryInfo
    {
        protected uint mAddress;
        public uint Address
        {
            get
            {
                return mAddress;
            }
            set
            {
                mAddress = value;
            }
        }

        protected TypeInfo mTypeInfo;
        public TypeInfo TypeInfo
        {
            get
            {
                return mTypeInfo;
            }
            set
            {
                mTypeInfo = value;
            }
        }

        protected TypeInfo mPureTypeInfo;
        public TypeInfo PureTypeInfo
        {
            get
            {
                return mPureTypeInfo;
            }
            set
            {
                mPureTypeInfo = value;
            }
        }

        protected bool mbChanged;
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
    }
}
