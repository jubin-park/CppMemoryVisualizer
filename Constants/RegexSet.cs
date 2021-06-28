using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Constants
{
    public sealed class RegexSet
    {
        public static readonly Regex REGEX_FRAME_ADDRESS = new Regex(@"^Stack frame at 0x([a-z0-9]+):$");
        public static readonly Regex REGEX_FUNCTION_ADDRESS = new Regex(@"^\seip\s=\s0x([a-z0-9]+)\sin\s");
        public static readonly Regex REGEX_FUNCTION_SIGNATURE = new Regex(@"^((.*)\s\+\s(\d+)|(.*))\sin\ssection\s");
        public static readonly Regex REGEX_LOCAL_ADDRESS = new Regex(@"\s0x([a-z0-9]+)");
        public static readonly Regex REGEX_ONE_LINE_TYPE = new Regex(@"([a-zA-Z0-9_<>,: ]+)($|\s(\**)([\(\*+\)]*)([\[\d+\]]*)(&{0,1}))");
        public static readonly Regex REGEX_CLASS_STRUCT_ENUM_UNION_TYPE = new Regex(@"(class|struct|enum|union)\s{0,1}(.*)\s{$");
        
        public static readonly Regex REGEX_BLOCK_TYPE_FOOTER = new Regex(@"}\s(\w+);");
        public static readonly Regex REGEX_MEMBER_NAME = new Regex(@"[a-zA-Z_$][a-zA-Z_$0-9]*", RegexOptions.RightToLeft);

        public static readonly Regex REGEX_TYPE_TOTAL_SIZE = new Regex(@"\/\*\stotal\ssize\s\(bytes\):\s*(\d+)\s\*\/$");
        public static readonly Regex REGEX_OFFSET_AND_SIZE = new Regex(@"^\/\*\s*((\d+)\s*\|){0,1}\s*(\d+)\s*\*\/");

        public static readonly Regex REGEX_CLASS_OR_STRUCT = new Regex(@"(class|struct)\s{0,1}(.*)\s{$"); // $1 = class|struct $2 = remain including inheritance
        public static readonly Regex REGEX_INHERITANCE = new Regex(@"(^|,\s)(public|protected|private)\s");
        public static readonly Regex REGEX_ENUM = new Regex(@"enum.*\s([a-zA-Z_$][a-zA-Z_$0-9]*);$");
        public static readonly Regex REGEX_NAMED_ENUM = new Regex(@"^enum\s(.*)\s:\s.*{(.*)}$");
        public static readonly Regex REGEX_UNION = new Regex(@"union {$");

        public static readonly Regex REGEX_DERIVED_REAL_TYPE = new Regex(@"^type\s=\s\/\*\sreal\stype\s=\s(.*)\s\*\/$");
    }
}
