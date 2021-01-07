using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Models
{
    sealed class MemberInfo
    {
        public string Name;
        public uint Offset;
        public uint Size;
        public uint UnitSize;
        public string Type;
    }
}
