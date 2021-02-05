using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.ViewModels
{
    class StackMemorySegmentViewModel
    {
        public string TypeName { get; set; }
        public string MemberNameOrNull { get; set; }
        public ArraySegment<byte> Memory { get; set; }
        public uint Address { get; set; }
        public StackMemorySegmentViewModel AncestorOrNull { get; set; }
        public List<List<StackMemorySegmentViewModel>> Children { get; set; }
    }
}
