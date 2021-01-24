using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Constants
{
    public sealed class GdbInstructionSet
    {
        public static readonly string RUN = "run";
        public static readonly string QUIT = "q";
        
        public static readonly string STEP_OVER = "next";
        public static readonly string STEP_IN = "step";
        public static readonly string GO = "continue";
        public static readonly string FINISH = "finish";

        public static readonly string DISPLAY_STACK_BACKTRACE = "info stack";
        public static readonly string DISPLAY_LOCAL_VARIABLE = "info locals";

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/dt--display-type-
        public static readonly string DISPLAY_TYPE = "dt -i {0}";

        public static readonly string ADD_BREAK_POINT = "break {0}:{1}"; // src_name:line
        public static readonly string REMOVE_BREAK_POINT = "clear {0}:{1}"; // src_name:line
        public static readonly string SET_BREAK_POINT_MAIN = "b main";
        public static readonly string CLEAR_ALL_BREAK_POINTS = "delete";

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/-address
        public static readonly string DISPLAY_ADDRESS = "!address {0}";

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/-heap
        public static readonly string DISPLAY_HEAP = "!heap -x {0}";

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/dx--display-visualizer-variables-
        public static readonly string DISPLAY_EXPRESSION = "dx {0}";

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/d--da--db--dc--dd--dd--df--dp--dq--du--dw--dw--dyb--dyd--display-memor
        public static readonly string DISPLAY_MEMORY = "dd /c{0} {1} L{0}";

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/----evaluate-c---expression-
        public static readonly string EVALUATE_SIZEOF = "?? sizeof({0})";

        // https://sourceware.org/gdb/current/onlinedocs/gdb/Output.html
        // https://ftp.gnu.org/old-gnu/Manuals/gdb/html_node/gdb_57.html#SEC58
        public static readonly string PRINTF = "printf \"{0}\\n\"";

        // https://sourceware.org/gdb/onlinedocs/gdb/Skipping-Over-Functions-and-Files.html
        public static readonly string SKIP_STL_CONSTRUCTOR_DESTRUCTOR = @"skip -rfu ^std::([a-zA-z0-9_]+)<.*>::~?\1 .*\(";

        #region Request by Echo
        public static readonly string OUTPUT_HEADER = "(gdb) ";
        public static readonly string REQUEST_START_CONSOLE = "@S=CONSOLE";
        public static readonly string REQUEST_END_CONSOLE = "@E=CONSOLE";

        public static readonly string REQUEST_START = "@S=";
        public static readonly string REQUEST_END = "@E=";

        public static readonly string REQUEST_START_INIT = "@S=INIT";
        public static readonly string REQUEST_END_INIT = "@E=INIT";

        public static readonly string REQUEST_START_GET_CALL_STACK = "@S=GET_CALL_STACK";
        public static readonly string REQUEST_END_GET_CALL_STACK = "@E=GET_CALL_STACK";

        public static readonly string REQUEST_START_GET_LOCAL_VARS = "@S=GET_LOCAL_VARS";
        public static readonly string REQUEST_END_GET_LOCAL_VARS = "@E=GET_LOCAL_VARS";

        public static readonly string REQUEST_START_SIZEOF = "@S=SIZEOF";
        public static readonly string REQUEST_END_SIZEOF = "@E=SIZEOF";

        public static readonly string REQUEST_START_DISPLAY_MEMORY = "@S=DM";
        public static readonly string REQUEST_END_DISPLAY_MEMORY = "@E=DM";

        public static readonly string REQUEST_START_DISPLAY_TYPE = "@S=DT";
        public static readonly string REQUEST_END_DISPLAY_TYPE = "@E=DT";

        public static readonly string REQUEST_START_HEAP = "@S=HEAP";
        public static readonly string REQUEST_END_HEAP = "@E=HEAP";

        public static readonly string REQUEST_START_GO_COMMAND = "@S=GO";
        public static readonly string REQUEST_END_GO_COMMAND = "@E=GO";

        public static readonly string REQUEST_START_FINISH_COMMAND = "@S=FINISH";
        public static readonly string REQUEST_END_FINISH_COMMAND = "@E=FINISH";

        public static readonly string REQUEST_START_STEP_OVER_COMMAND = "@S=STEP_OVER";
        public static readonly string REQUEST_END_STEP_OVER_COMMAND = "@E=STEP_OVER";

        public static readonly string REQUEST_START_STEP_IN_COMMAND = "@S=STEP_IN";
        public static readonly string REQUEST_END_STEP_IN_COMMAND = "@E=STEP_IN";

        public static readonly string REQUEST_START_ADD_BREAK_POINT = "@S=DBPL";
        public static readonly string REQUEST_END_ADD_BREAK_POINT = "@E=DBPL";
        #endregion
    }
}
