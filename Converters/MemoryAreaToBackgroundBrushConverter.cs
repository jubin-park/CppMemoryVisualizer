using CppMemoryVisualizer.Enums;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CppMemoryVisualizer.Converters
{
    class MemoryAreaToBackgroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var memoryArea = (EMemoryArea)value;

            switch (memoryArea)
            {
                case EMemoryArea.CALL_STACK:
                    return new SolidColorBrush(Colors.MidnightBlue);

                case EMemoryArea.HEAP:
                    return new SolidColorBrush(Colors.Yellow);

                case EMemoryArea.UNKNOWN:
                /* intentional fallthrough */
                default:
                    return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
