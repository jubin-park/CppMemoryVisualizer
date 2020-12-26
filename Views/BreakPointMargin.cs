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
            Size renderSize = this.RenderSize;

            if (textView != null && textView.VisualLinesValid)
            {
                foreach (VisualLine line in textView.VisualLines)
                {
                    int lineNumber = line.FirstDocumentLine.LineNumber;
                    Debug.Write(lineNumber + " ");
                }
                Debug.WriteLine("");
            }

            drawingContext.DrawRectangle(Brushes.LightGray, null, new Rect(0, 0, MARGIN_WIDTH, RenderSize.Height));
        }

        protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
        {
            InvalidateVisual();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(MARGIN_WIDTH, RenderSize.Height);
        }
    }
}
