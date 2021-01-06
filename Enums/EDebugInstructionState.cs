﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer
{
    public enum EDebugInstructionState
    {
        STANDBY,
        INIT,
        DEBUG,
        STEP_IN,
        STEP_OVER,
        GO,
        ADD_BREAK_POINT,
        REMOVE_BREAK_POINT,
    }
}
