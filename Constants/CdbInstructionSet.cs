using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Constants
{
    public sealed class CdbInstructionSet
    {
        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/q--qq--quit-
        public static readonly string QUIT = "q";

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/p--step-
        public static readonly string STEP_OVER = "p";

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/t--trace-
        public static readonly string STEP_IN = "t";

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/g--go-
        public static readonly string GO = "g";

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/k--kb--kc--kd--kp--kp--kv--display-stack-backtrace-
        public static readonly string DISPLAY_STACK_BACKTRACE = "kn";

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/dv--display-local-variables-
        public static readonly string DISPLAY_LOCAL_VARIABLE = "dv /i /t /a /v";

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/dt--display-type-
        public static readonly string DISPLAY_TYPE = "dt -i {0}";

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/bl--breakpoint-list-
        public static readonly string DISPLAY_BREAK_POINT_LIST = "bl";

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/bp--bu--bm--set-breakpoint-
        public static readonly string SET_BREAK_POINT_SOURCE_LEVEL = "bp (@@masm(`{0}:{1}`))";
        public static readonly string SET_BREAK_POINT_MAIN = "bu {0}!main";

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/bc--breakpoint-clear-
        public static readonly string CLEAR_BREAK_POINT = "bc {0}";
        public static readonly string CLEAR_BREAK_POINT_MAIN = "bc \"{0}!main\"";

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

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/-expr--choose-expression-evaluator-
        public static readonly string CPP_EXPRESSION_EVALUATOR = ".expr /s c++";
        
        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/-lines--toggle-source-line-support-
        public static readonly string ENABLE_SOURCE_LINE_SUPPORT = ".lines -e";
        
        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/l---l---set-source-options-
        public static readonly string SET_SOURCE_OPTIONS = "l+*";

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/-settings--set-debug-settings-
        public static readonly string SET_DEBUG_SETTINGS_SKIP_CRT_CODE = ".settings set Sources.SkipCrtCode=true";

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/-echo--echo-comment-
        public static readonly string ECHO = ".echo \"{0}\"";

        #region Request by Echo
        public static readonly string OUTPUT_HEADER = "0:000> ";

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

        public static readonly string REQUEST_START_STEP_OVER_COMMAND = "@S=STEP_OVER";
        public static readonly string REQUEST_END_STEP_OVER_COMMAND = "@E=STEP_OVER";

        public static readonly string REQUEST_START_STEP_IN_COMMAND = "@S=STEP_IN";
        public static readonly string REQUEST_END_STEP_IN_COMMAND = "@E=STEP_IN";

        public static readonly string REQUEST_START_DISPLAY_BREAK_POINT_LIST = "@S=DBPL";
        public static readonly string REQUEST_END_DISPLAY_BREAK_POINT_LIST = "@E=DBPL";
        #endregion
    }
}
