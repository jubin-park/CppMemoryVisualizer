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

        public static readonly string ADD_BREAK_POINT = "break {0}:{1}"; // <src_name.cpp>:line
        public static readonly string REMOVE_BREAK_POINT = "clear {0}:{1}"; // <src_name.cpp>:line
        public static readonly string SET_BREAK_POINT_MAIN = "b main";
        public static readonly string CLEAR_ALL_BREAK_POINTS = "delete";

        public static readonly string DISPLAY_STACK_BACKTRACE = "info stack";
        public static readonly string DISPLAY_INFO_FRAME = "info frame {0}"; // index
        public static readonly string DISPLAY_ARGUMENTS = "info args -q";
        public static readonly string DISPLAY_LOCAL_VARIABLES = "info locals -q";
        public static readonly string DISPLAY_ADDRESS = "call &{0}"; // <name>
        public static readonly string DISPLAY_TYPE_NAME = "whatis {0}"; // <name>
        public static readonly string DISPLAY_TYPE_INFO = "ptype /o {0}"; // <name>
        public static readonly string DISPLAY_SIZEOF = "call sizeof({0})"; // 0xADDRESS | <name>
        public static readonly string DISPLAY_INFO_SYMBOL = "info symbol {0}"; // 0xADDRESS | <name>
        public static readonly string DISPLAY_MEMORY = "x/{0}x {1}"; // count, 0xADDRESS
        public static readonly string DISPLAY_HEAP_SIZEOF = "call (int)'msvcrt!_msize'({0})"; // 0xADDRESS

        public static readonly string SELECT_FRAME = "select-frame {0}"; // index

        // https://sourceware.org/gdb/current/onlinedocs/gdb/Output.html
        // https://ftp.gnu.org/old-gnu/Manuals/gdb/html_node/gdb_57.html#SEC58
        public static readonly string PRINTF = "printf \"{0}\\n\"";

        public static readonly string SET_PAGINATION_OFF = "set pagination off";

        #region Request by Echo
        public static readonly string OUTPUT_HEADER = "(gdb) ";
        public static readonly string REQUEST_START_CONSOLE = "@S=CONSOLE";
        public static readonly string REQUEST_END_CONSOLE = "@E=CONSOLE";

        public static readonly string REQUEST_START = "@S=";
        public static readonly string REQUEST_END = "@E=";

        public static readonly string REQUEST_START_INIT = "@S=INIT";
        public static readonly string REQUEST_END_INIT = "@E=INIT";

        public static readonly string REQUEST_START_DISPLAY_CALL_STACK = "@S=DISPLAY_CALL_STACK";
        public static readonly string REQUEST_END_DISPLAY_CALL_STACK = "@E=DISPLAY_CALL_STACK";

        public static readonly string REQUEST_START_DISPLAY_INFO_FRAME = "@S=DISPLAY_INFO_FRAME";
        public static readonly string REQUEST_END_DISPLAY_INFO_FRAME = "@E=DISPLAY_INFO_FRAME";

        public static readonly string REQUEST_START_DISPLAY_ARGUMENTS = "@S=DISPLAY_ARGS";
        public static readonly string REQUEST_END_DISPLAY_ARGUMENTS = "@E=DISPLAY_ARGS";

        public static readonly string REQUEST_START_DISPLAY_LOCAL_VARIABLES = "@S=DISPLAY_LOCAL_VARS";
        public static readonly string REQUEST_END_DISPLAY_LOCAL_VARIABLES = "@E=DISPLAY_LOCAL_VARS";

        public static readonly string REQUEST_START_DISPLAY_ADDRESS = "@S=DISPLAY_ADDRESS";
        public static readonly string REQUEST_END_DISPLAY_ADDRESS = "@E=DISPLAY_ADDRESS";

        public static readonly string REQUEST_START_DISPLAY_TYPE = "@S=DT";
        public static readonly string REQUEST_END_DISPLAY_TYPE = "@E=DT";

        public static readonly string REQUEST_START_DISPLAY_SIZEOF = "@S=SIZEOF";
        public static readonly string REQUEST_END_DISPLAY_SIZEOF = "@E=SIZEOF";

        public static readonly string REQUEST_START_DISPLAY_INFO_SYMBOL = "@S=DISPLAY_INFO_SYMBOL";
        public static readonly string REQUEST_END_DISPLAY_INFO_SYMBOL = "@E=DISPLAY_INFO_SYMBOL";

        public static readonly string REQUEST_START_DISPLAY_MEMORY = "@S=DM";
        public static readonly string REQUEST_END_DISPLAY_MEMORY = "@E=DM";

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
