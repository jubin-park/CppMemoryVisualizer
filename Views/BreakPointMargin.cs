using CppMemoryVisualizer.ViewModels;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;

namespace CppMemoryVisualizer.Views
{
    sealed class BreakPointMargin : AbstractMargin
    {
        private static int MARGIN_WIDTH = 24;

        private BindableAvalonEditor mEditor;
        private MainViewModel mMainViewModel;

        public BindableAvalonEditor Editor
        {
            get
            {
                return mEditor;
            }
        }

        public BreakPointMargin(BindableAvalonEditor editor)
        {
            Debug.Assert(editor != null);

            mEditor = editor;
            mMainViewModel = (MainViewModel)editor.DataContext;

            mMainViewModel.LinePointerChanged += new PropertyChangedEventHandler(onLinePointerChanged);
            MouseLeftButtonDown += onMouseLeftButtonDown;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            TextView textView = this.TextView;
            Size renderSize = this.RenderSize;
            var breakPointInfoOrNull = mMainViewModel.BreakPointList;

            drawingContext.DrawRectangle(Brushes.LightGray, null, new Rect(0, 0, MARGIN_WIDTH, RenderSize.Height));

            if (breakPointInfoOrNull != null && textView != null && textView.VisualLinesValid)
            {
                foreach (VisualLine line in textView.VisualLines)
                {
                    int lineNumber = line.FirstDocumentLine.LineNumber;
                    double y = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop);

                    if (breakPointInfoOrNull.Indices[lineNumber])
                    {
                        drawingContext.DrawRectangle(Brushes.Red, null, new Rect(7, 3 + y - textView.VerticalOffset, 9, 9));
                    }
                }

                foreach (VisualLine line in textView.VisualLines)
                {
                    int lineNumber = line.FirstDocumentLine.LineNumber;
                    double y = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop);

                    if ((uint)lineNumber == mMainViewModel.LinePointer)
                    {
                        drawingContext.DrawRectangle(Brushes.Yellow, null, new Rect(7, 5 + y - textView.VerticalOffset, 9, 4));
                        break;
                    }
                }
            }
        }

        void TextViewVisualLinesChanged(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
        {
            if (oldTextView != null)
            {
                oldTextView.VisualLinesChanged -= TextViewVisualLinesChanged;
            }
            base.OnTextViewChanged(oldTextView, newTextView);
            if (newTextView != null)
            {
                newTextView.VisualLinesChanged += TextViewVisualLinesChanged;
            }

            InvalidateVisual();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(MARGIN_WIDTH, RenderSize.Height);
        }

        private void onMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var positionOrNull = mEditor.GetPositionFromPoint(e.GetPosition(this));
            if (positionOrNull == null)
            {
                return;
            }

            uint line = (uint)positionOrNull.Value.Location.Line;

            if (mMainViewModel.AddOrRemoveBreakPointCommand.CanExecute(line))
            {
                mMainViewModel.AddOrRemoveBreakPointCommand.Execute(line);
                InvalidateVisual();
            }
        }

        private void onLinePointerChanged(object sender, PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                InvalidateVisual();
                double vertOffset = (this.TextView.DefaultLineHeight) * (mMainViewModel.LinePointer - 10);
                mEditor.ScrollToVerticalOffset(vertOffset);
            });
        }
    }
}
