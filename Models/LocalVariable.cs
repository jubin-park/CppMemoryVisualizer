using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Models
{
    class LocalVariable
    {
        private string mAddress = string.Empty;
        public string Address
        {
            get { return mAddress; }
            set { mAddress = value; }
        }

        private string mType = string.Empty;
        public string Type
        {
            get { return mType; }
            set { mType = value; }
        }

        private string mName = string.Empty;
        public string Name
        {
            get { return mName; }
            set { mName = value; }
        }

        private string mValue = string.Empty;
        public string Value
        {
            get { return mValue; }
            set { mValue = value; }
        }

        private uint mSize = uint.MaxValue;
        public uint Size
        {
            get { return mSize; }
            set { mSize = value; }
        }
    }
}
