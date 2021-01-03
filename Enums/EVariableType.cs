using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Enums
{
    public enum EVariableType
    {
        CHAR = 1,
        SHORT = 2,
        INT = 4,
        UCHAR = 1,
        USHORT = 2,
        UINT = 4,

        FLOAT = 4,
        DOUBLE = 4,

        POINTER = 4,
        
        CLASS,
        STRUCT,
    }
}
