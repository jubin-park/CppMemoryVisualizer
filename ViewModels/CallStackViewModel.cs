using CppMemoryVisualizer.Models;
using System.ComponentModel;

namespace CppMemoryVisualizer.ViewModels
{
    class CallStackViewModel : INotifyPropertyChanged
    {
        private CallStack mCallStack = new CallStack();
        public CallStack CallStack
        {
            get
            {
                return mCallStack;
            }
            set
            {
                mCallStack = value;
                onPropertyChanged("CallStack");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
