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
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;

namespace CppMemoryVisualizer.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel mMainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            mMainViewModel = (MainViewModel)DataContext;
            Closing += OnWindowClosing;
            xTextEditor.TextArea.LeftMargins.Insert(0, new BreakPointMargin());
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            mMainViewModel.ShutdownCdb();
        }

        private void xTextBoxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            xTextBoxLog.CaretIndex = xTextBoxLog.Text.Length;
            xTextBoxLog.ScrollToEnd();
        }

        private void xTextBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && xTextBoxInput.Text.Length > 0 && mMainViewModel.SendInstruction(xTextBoxInput.Text))
            {
                xTextBoxInput.Text = string.Empty;
            }
        }
    }
}
