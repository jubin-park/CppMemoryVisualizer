using CppMemoryVisualizer.ViewModels;

namespace CppMemoryVisualizer.Commands
{
    class MemorySegmentAddressClickCommand : MemorySegmentClickCommand
    {
        public MemorySegmentAddressClickCommand(MainViewModel mainViewModel)
            : base(mainViewModel)
        {
        }

        public override void Execute(object parameter)
        {
            base.Execute((uint)parameter);
        }
    }
}
