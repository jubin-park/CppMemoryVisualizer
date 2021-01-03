using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Models
{
    class LocalVariable
    {
        private string mName = string.Empty;
        public string Name
        {
            get { return mName; }
            set { mName = value; }
        }

        private MemoryInfo mStackMemory = new MemoryInfo();
        public MemoryInfo StackMemory
        {
            get { return mStackMemory; }
        }
    }
}
