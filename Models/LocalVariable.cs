using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Models
{
    class LocalVariable
    {
        private string mAddress;
        public string Address
        {
            get { return mAddress; }
            set { mAddress = value; }
        }

        private string mType;
        public string Type
        {
            get { return mType; }
            set { mType = value; }
        }

        private string mName;
        public string Name
        {
            get { return mName; }
            set { mName = value; }
        }

        private string mValue;
        public string Value
        {
            get { return mValue; }
            set { mValue = value; }
        }

        private uint mSize;
        public uint Size
        {
            get { return mSize; }
            set { mSize = value; }
        }
    }
}
