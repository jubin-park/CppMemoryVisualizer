using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer
{
    public enum EDebugInstructionState
    {
        NULL,
        STEP_IN,
        STEP_OVER,
        GO,
        ADD_BREAK_POINT,
        REMOVE_BREAK_POINT,
    }
}
