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

namespace CppMemoryVisualizer
{
    class BreakPointMargin : AbstractMargin
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            Size renderSize = this.RenderSize;
            drawingContext.DrawRectangle(Brushes.Red, null, new Rect(0, 0, 12, 12));
        }
    }
}
