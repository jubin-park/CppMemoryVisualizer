using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using Microsoft.Win32;
using CppMemoryVisualizer.ViewModels;

namespace CppMemoryVisualizer
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel mMainViewModel;
        public MainWindow()
        {
            InitializeComponent();
            mMainViewModel = (MainViewModel)DataContext;
            Closing += OnWindowClosing;
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Process processOrNull = mMainViewModel.ProcessCdbOrNull;
            if (processOrNull != null)
            {
                processOrNull.StandardInput.WriteLine("q");
                processOrNull.WaitForExit();
            }

            Thread threadOrNull = mMainViewModel.ThreadCdbOrNull;
            if (threadOrNull != null)
            {
                threadOrNull.Join();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (mMainViewModel.SendInstruction(xTextInput.Text))
            {
                xTextInput.Text = string.Empty;
            }
        }
    }
}
