using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace CppMemoryVisualizer.Views
{
    sealed class WindowTitleConverter : IMultiValueConverter
    {
        private static string WINDOW_TITLE = "C++ Memory Visualizer";

        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            string srcPathOrNull = value[0] as string;
            if (srcPathOrNull == null || srcPathOrNull == string.Empty)
            {
                return WINDOW_TITLE;
            }

            Process processOrNull = value[1] as Process;
            if (processOrNull == null)
            {
                return string.Format("{0} ( No Debugger ) - @[{1}]", WINDOW_TITLE, srcPathOrNull);
            }

            EDebugInstructionState state = (EDebugInstructionState)value[2];

            return string.Format("{0} ( {1} ) - @[{2}]", WINDOW_TITLE, state.ToString(), srcPathOrNull);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            return new object[] { null, null, null };
        }
    }
}
