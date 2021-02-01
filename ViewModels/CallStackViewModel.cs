using CppMemoryVisualizer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
