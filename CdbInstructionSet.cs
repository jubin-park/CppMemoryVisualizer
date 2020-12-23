using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer
{
    public sealed class CdbInstructionSet
    {
        public static readonly string QUIT = "q";
        public static readonly string STEP_OVER = "p";
        public static readonly string STEP_IN = "t";
        public static readonly string RESUME = "g";
        public static readonly string BREAK_POINT = "bp (@@masm(`{0}:{1}+`))";
    }
}
