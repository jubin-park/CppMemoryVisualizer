using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CppMemoryVisualizer.ViewModels;
using CppMemoryVisualizer.Constants;
using System.Windows.Controls.Primitives;
using CppMemoryVisualizer.Models;

namespace CppMemoryVisualizer.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel mMainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            mMainViewModel = (MainViewModel)DataContext;
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            mMainViewModel.ShutdownGdb();
        }

        private void xTextBoxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            xTextBoxLog.CaretIndex = xTextBoxLog.Text.Length;
            xTextBoxLog.ScrollToEnd();
        }

        private void xTextBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && xTextBoxInput.Text.Length > 0)
            {
                mMainViewModel.RequestInstruction(xTextBoxInput.Text,
                    GdbInstructionSet.REQUEST_START_CONSOLE, GdbInstructionSet.REQUEST_END_CONSOLE);
                mMainViewModel.ReadResultLine(GdbInstructionSet.REQUEST_START_CONSOLE, GdbInstructionSet.REQUEST_END_CONSOLE, null);

                xTextBoxInput.Text = string.Empty;
            }
        }
    }
}
