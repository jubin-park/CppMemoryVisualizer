﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer
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

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/bp--bu--bm--set-breakpoint-
        public static readonly string SET_BREAK_POINT_SOURCE_LEVEL = "bp (@@masm(`{0}:{1}+`))";
        public static readonly string SET_BREAK_POINT_MAIN = "bu {0}!main";

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/-expr--choose-expression-evaluator-
        public static readonly string CPP_EXPRESSION_EVALUATOR = ".expr /s c++";
        
        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/-lines--toggle-source-line-support-
        public static readonly string ENABLE_SOURCE_LINE_SUPPORT = ".lines -e";
        
        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/l---l---set-source-options-
        public static readonly string SET_SOURCE_OPTIONS = "l+*";

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/-settings--set-debug-settings-
        public static readonly string SET_DEBUG_SETTINGS_SKIP_CRT_CODE = ".settings set Sources.SkipCrtCode=true";
    }
}