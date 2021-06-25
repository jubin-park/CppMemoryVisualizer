using CppMemoryVisualizer.ViewModels;
using System.Diagnostics;
using System.Linq;

namespace CppMemoryVisualizer.Commands
{
    class MemorySegmentPointerValueClickCommand : MemorySegmentClickCommand
    {
        public MemorySegmentPointerValueClickCommand(MainViewModel mainViewModel)
            : base(mainViewModel)
        {
        }

        public override void Execute(object parameter)
        {
            var vm = (MemorySegmentViewModel)parameter;
            Debug.Assert(vm != null);
            Debug.Assert(vm.Memory != null);

            if (null != vm.TypeName && vm.TypeName.Contains('*') && vm.Memory.Count == 4)
            {
                uint targetAddress = (uint)vm.Memory.Array[vm.Memory.Offset] |
                    (uint)vm.Memory.Array[vm.Memory.Offset + 1] << 8 |
                    (uint)vm.Memory.Array[vm.Memory.Offset + 2] << 16 |
                    (uint)vm.Memory.Array[vm.Memory.Offset + 3] << 24;

                base.Execute(targetAddress);
            }
        }
    }
}
