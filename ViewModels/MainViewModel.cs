using CppMemoryVisualizer.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CppMemoryVisualizer.ViewModels
{
    class MainViewModel
    {
        public ICommand LoadSourceFileCommand { get; }

        public MainViewModel()
        {
            LoadSourceFileCommand = new LoadSourceFileCommand();
        }
    }
}
