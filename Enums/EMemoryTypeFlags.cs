using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Enums
{
    [Flags]
    public enum EMemoryTypeFlags : short
    {
        POINTER = 1 << 0,
        ARRAY = 1 << 1,
        
        ENUM = 1 << 2,
        UNION = 1 << 3,
        STRUCT = 1 << 4,
        CLASS = 1 << 5,
        FUNCTION = 1 << 6,

        PARAMETER = 1 << 7,
        REFERENCE = 1 << 8,
        STL = 1 << 9,
    }
}
