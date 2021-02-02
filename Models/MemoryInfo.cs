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

        protected readonly TypeInfo mTypeInfo = new TypeInfo();
        public TypeInfo TypeInfo
        {
            get
            {
                return mTypeInfo;
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
    }
}
