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
        public static readonly string DISPLAY_TYPENAME = "whatis /r {0}"; // <name>
        public static readonly string DISPLAY_TYPEINFO = "ptype /o {0}"; // <name>
        public static readonly string DISPLAY_SIZEOF = "call sizeof({0})"; // 0xADDRESS | <name>
        public static readonly string DISPLAY_SYMBOLINFO = "info symbol {0}"; // 0xADDRESS | <name>
        public static readonly string DISPLAY_MEMORY = "x/{0}x {1}"; // count, 0xADDRESS
        public static readonly string DISPLAY_MEMBER_OFFSET = "p &(({0}*)0)->{1}"; // symbolname, membername
        public static readonly string DISPLAY_MEMBER_TYPE = "whatis *&(({0}*)0)->{1}"; // symbolname, membername

        public static readonly string SELECT_FRAME = "select-frame {0}"; // index

        public static readonly string PRINTF = "printf \"{0}\\n\"";
        
        public static readonly string UNLIMITED_NESTED_TYPE = "set print type nested-type-limit unlimited";
        public static readonly string CREATE_HEAPINFO = "create_heapinfo";
        public static readonly string DISPLAY_HEAPINFO = "display_heapinfo";
        public static readonly string SET_UNWINDONSIGNAL_ON = "set unwindonsignal on";
        public static readonly string SET_PAGINATION_OFF = "set pagination off";
        public static readonly string DEFINE_COMMANDS =
$@"define {CREATE_HEAPINFO}
    set $heapinfo = (int*)malloc(sizeof(int)*3)
    printf ""$heapinfo is created: 0x%08x\n"", $heapinfo
end
define {DISPLAY_HEAPINFO}
    set *$heapinfo = 0
    while(1)
        set $ret=(int)'msvcrt!_heapwalk'($heapinfo)
        if($ret!=-2)
            loop_break
        end
        printf ""%08x%08x%d\n"", (int*)$heapinfo[0], $heapinfo[1], $heapinfo[2]
    end
end"; // _pentry, _size, _useflag

        public static readonly string OUTPUT_HEADER = "(gdb) ";
        public static readonly string REQUEST_START_CONSOLE = "@S=CONSOLE";
        public static readonly string REQUEST_END_CONSOLE = "@E=CONSOLE";

        public static readonly string REQUEST_START = "@S=";
        public static readonly string REQUEST_END = "@E=";

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

        public static readonly string REQUEST_START_DISPLAY_SYMBOLINFO = "@S=DISPLAY_SYMBOLINFO";
        public static readonly string REQUEST_END_DISPLAY_SYMBOLINFO = "@E=DISPLAY_SYMBOLINFO";

        public static readonly string REQUEST_START_DISPLAY_MEMORY = "@S=DM";
        public static readonly string REQUEST_END_DISPLAY_MEMORY = "@E=DM";

        public static readonly string REQUEST_START_DISPLAY_HEAPINFO = "@S=HEAPINFO";
        public static readonly string REQUEST_END_DISPLAY_HEAPINFO = "@E=HEAPINFO";

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
    }
}
