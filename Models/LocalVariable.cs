using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Models
{
    sealed class LocalVariable
    {
        public bool IsParameter;

        private string mName;
        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                mName = value;
            }
        }

        private readonly MemoryOwnerInfo mStackMemory = new MemoryOwnerInfo();
        public MemoryOwnerInfo StackMemory
        {
            get
            {
                return mStackMemory;
            }
        }
    }
}
