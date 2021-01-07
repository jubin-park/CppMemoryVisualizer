using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CppMemoryVisualizer.Views
{
    public sealed class BindableAvalonEditor : ICSharpCode.AvalonEdit.TextEditor, INotifyPropertyChanged
    {
        public int[] BreakPointIndices
        {
            get { return (int[])GetValue(BreakPointIndicesProperty); }
            set { SetValue(BreakPointIndicesProperty, value); }
        }

        public static readonly DependencyProperty BreakPointIndicesProperty =
            DependencyProperty.Register("BreakPointIndices", typeof(int[]), typeof(BindableAvalonEditor), new PropertyMetadata(null));

        public uint LinePointer
        {
            get { return (uint)GetValue(LinePointerProperty); }
            set { SetValue(LinePointerProperty, value); RaisePropertyChanged("LinePointer"); }
        }

        public static readonly DependencyProperty LinePointerProperty =
            DependencyProperty.Register("LinePointer", typeof(uint), typeof(BindableAvalonEditor), new PropertyMetadata(0u));

        #region ShowLineNumbersCustom
        /// <summary>
        /// ShowLineNumbersCustom dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowLineNumbersCustomProperty =
            DependencyProperty.Register("ShowLineNumbersCustom", typeof(bool), typeof(BindableAvalonEditor),
                                        new FrameworkPropertyMetadata(false, OnShowLineNumbersCustomChanged));

        /// <summary>
        /// Specifies whether line numbers are shown on the left to the text view.
        /// </summary>
        public bool ShowLineNumbersCustom
        {
            get { return (bool)GetValue(ShowLineNumbersCustomProperty); }
            set { SetValue(ShowLineNumbersCustomProperty, value); }
        }

        static void OnShowLineNumbersCustomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BindableAvalonEditor editor = (BindableAvalonEditor)d;
            var leftMargins = editor.TextArea.LeftMargins;
            if ((bool)e.NewValue)
            {
                LineNumberMargin lineNumbers = new LineNumberMargin();
                Line line = (Line)DottedLineMargin.Create();
                leftMargins.Insert(0, new BreakPointMargin(editor));
                leftMargins.Insert(1, lineNumbers);
                leftMargins.Insert(2, line);
                var lineNumbersForeground = new Binding("LineNumbersForeground") { Source = editor };
                line.SetBinding(Line.StrokeProperty, lineNumbersForeground);
                lineNumbers.SetBinding(Control.ForegroundProperty, lineNumbersForeground);
            }
            else
            {
                for (int i = 0; i < leftMargins.Count; i++)
                {
                    if (leftMargins[i] is LineNumberMargin)
                    {
                        leftMargins.RemoveAt(i);
                        if (i < leftMargins.Count && DottedLineMargin.IsDottedLineMargin(leftMargins[i]))
                        {
                            leftMargins.RemoveAt(i);
                        }
                        break;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// A bindable Text property
        /// </summary>
        public new string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
                RaisePropertyChanged("Text");
            }
        }

        /// <summary>
        /// The bindable text property dependency property
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(BindableAvalonEditor),
                new FrameworkPropertyMetadata
                {
                    DefaultValue = default(string),
                    BindsTwoWayByDefault = true,
                    PropertyChangedCallback = OnDependencyPropertyChanged
                }
            );

        protected static void OnDependencyPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var target = (BindableAvalonEditor)obj;

            if (target.Document != null)
            {
                var caretOffset = target.CaretOffset;
                var newValue = args.NewValue;

                if (newValue == null)
                {
                    newValue = string.Empty;
                }

                target.Document.Text = (string)newValue;
                target.CaretOffset = Math.Min(caretOffset, newValue.ToString().Length);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (Document != null)
            {
                Text = Document.Text;
            }

            base.OnTextChanged(e);
        }

        /// <summary>
        /// Raises a property changed event
        /// </summary>
        /// <param name="property">The name of the property that updates</param>
        public void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
