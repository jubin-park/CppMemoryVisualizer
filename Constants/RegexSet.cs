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
    }
}
