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
                readResultLine(GdbInstructionSet.REQUEST_START_CONSOLE, GdbInstructionSet.REQUEST_END_CONSOLE, null);

                xTextBoxInput.Text = string.Empty;
            }
        }

        private void xThumbHeap_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = (Thumb)sender;
            var heap = (HeapMemoryInfo)thumb.DataContext;

            heap.X += e.HorizontalChange;
            heap.Y += e.VerticalChange;
        }

        private void readResultLine(string start, string end, Action<string> lambdaOrNull)
        {
            Debug.Assert(start != null);
            Debug.Assert(end != null);

            string line;

            do
            {
                line = mMainViewModel.ProcessGdbOrNull.StandardOutput.ReadLine();
                {
                    int lastIndex = line.LastIndexOf(GdbInstructionSet.OUTPUT_HEADER);
                    if (lastIndex != -1)
                    {
                        line = line.Substring(lastIndex + GdbInstructionSet.OUTPUT_HEADER.Length);
                    }
                    if (line.Length == 0)
                    {
                        continue;
                    }
                }
#if DEBUG
                mMainViewModel.Log += line + Environment.NewLine;
#endif
            } while (!line.StartsWith(start));

            while (true)
            {
                line = mMainViewModel.ProcessGdbOrNull.StandardOutput.ReadLine();
                {
                    int lastIndex = line.LastIndexOf(GdbInstructionSet.OUTPUT_HEADER);
                    if (lastIndex != -1)
                    {
                        line = line.Substring(lastIndex + GdbInstructionSet.OUTPUT_HEADER.Length);
                    }
                    if (line.Length == 0)
                    {
                        continue;
                    }
                }
#if DEBUG
                mMainViewModel.Log += line + Environment.NewLine;
#endif
                if (line.StartsWith(end))
                {
                    break;
                }

                if (lambdaOrNull != null)
                {
                    lambdaOrNull.Invoke(line);
                }
            }
        }
    }
}
