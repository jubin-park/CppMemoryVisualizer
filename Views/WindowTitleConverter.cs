using System;
using System.Globalization;
using System.Windows.Data;

namespace CppMemoryVisualizer.Views
{
    class WindowTitleConverter : IValueConverter
    {
        private static string WINDOW_TITLE = "C++ Memory Visualizer";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string srcPath = value as string;

            if (srcPath == null || srcPath.Trim() == string.Empty)
            {
                return WINDOW_TITLE;
            }

            return string.Format("{0} - @[{1}]", WINDOW_TITLE, srcPath);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return WINDOW_TITLE;
        }
    }
}
