using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace CppMemoryVisualizer.Converters
{
    sealed class WindowTitleConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            string srcPathOrNull = value[0] as string;
            if (srcPathOrNull == null || srcPathOrNull == string.Empty)
            {
                return App.WINDOW_TITLE;
            }

            Process processOrNull = value[1] as Process;
            if (processOrNull == null)
            {
                return string.Format("{0} ( No Debugger ) - @[{1}]", App.WINDOW_TITLE, srcPathOrNull);
            }

            EDebugInstructionState state = (EDebugInstructionState)value[2];

            return string.Format("{0} ( {1} ) - @[{2}]", App.WINDOW_TITLE, state.ToString(), srcPathOrNull);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            return new object[] { null, null, null };
        }
    }
}
