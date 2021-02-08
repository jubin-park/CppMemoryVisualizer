using System;
using System.Collections.Generic;

namespace CppMemoryVisualizer.ViewModels
{
    class MemorySegmentViewModel
    {
        public string TypeName { get; set; }
        public string MemberNameOrNull { get; set; }
        public ArraySegment<byte> Memory { get; set; }
        public uint Address { get; set; }
        public MemorySegmentViewModel AncestorOrNull { get; set; }
        public List<List<MemorySegmentViewModel>> Children { get; set; }
    }
}
