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
        private static readonly double ZOOM_DELTA = 0.001;
        private static readonly double ZOOM_MAX = 5.0;
        private static readonly double ZOOM_MIN = 0.2;

        private readonly MainViewModel mMainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            mMainViewModel = (MainViewModel)DataContext;
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

        private void xWindow_Closing(object sender, CancelEventArgs e)
        {
            mMainViewModel.ShutdownGdb();
        }

        private void xCallStackScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = (Keyboard.Modifiers == ModifierKeys.Control);

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                mMainViewModel.StackMemoryViewerZoom = Math.Min(ZOOM_MAX, Math.Max(ZOOM_MIN, mMainViewModel.StackMemoryViewerZoom + e.Delta * ZOOM_DELTA));
            }
        }

        private void xHeapScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = (Keyboard.Modifiers == ModifierKeys.Control);

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                mMainViewModel.HeapMemoryViewerZoom = Math.Min(ZOOM_MAX, Math.Max(ZOOM_MIN, mMainViewModel.HeapMemoryViewerZoom + e.Delta * ZOOM_DELTA));
            }
        }
    }
}
