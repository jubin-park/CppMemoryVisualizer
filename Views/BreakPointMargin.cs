using CppMemoryVisualizer.ViewModels;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;

namespace CppMemoryVisualizer.Views
{
    class BreakPointMargin : AbstractMargin
    {
        private static int MARGIN_WIDTH = 24;
        private BindableAvalonEditor mEditor;
        public BindableAvalonEditor Editor
        {
            get
            {
                return mEditor;
            }
        }

        public BreakPointMargin(BindableAvalonEditor editor)
        {
            mEditor = editor;
            MouseLeftButtonDown += MainViewModel.BreakPointMargin_OnMouseLeftButtonDown;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            TextView textView = this.TextView;
            MainViewModel mainViewModel = (MainViewModel)DataContext;
            Size renderSize = this.RenderSize;

            var breakPoints = mainViewModel.BreakPointLines;

            drawingContext.DrawRectangle(Brushes.LightGray, null, new Rect(0, 0, MARGIN_WIDTH, RenderSize.Height));

            if (breakPoints != null && textView != null && textView.VisualLinesValid)
            {
                foreach (VisualLine line in textView.VisualLines)
                {
                    int lineNumber = line.FirstDocumentLine.LineNumber;
                    double y = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop);

                    if (breakPoints[lineNumber] >= 0)
                    {
                        drawingContext.DrawRectangle(Brushes.Red, null, new Rect(7, 3 + y - textView.VerticalOffset, 9, 9));
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
    }
}
