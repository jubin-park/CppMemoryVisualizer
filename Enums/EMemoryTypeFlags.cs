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
        NONE = 0,
        POINTER = 1 << 0,
        ARRAY_OR_FUNCTION_POINTER = 1 << 1,
        ARRAY = 1 << 2,
        REFERENCE = 1 << 3,
        ENUM = 1 << 4,
        UNION = 1 << 5,
        STRUCT = 1 << 6,
        CLASS = 1 << 7,
        // https://stackoverflow.com/questions/48938003/windbg-c-how-to-print-vector-contents
        STL = 1 << 8,
    }
}
