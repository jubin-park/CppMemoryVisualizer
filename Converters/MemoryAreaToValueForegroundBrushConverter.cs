using CppMemoryVisualizer.Enums;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CppMemoryVisualizer.Converters
{
    class MemoryAreaToValueForegroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var memoryArea = (EMemoryArea)value;

            switch (memoryArea)
            {
                case EMemoryArea.CALL_STACK:
                    return new SolidColorBrush(Colors.White);

                case EMemoryArea.HEAP:
                    return new SolidColorBrush(Colors.Black);

                case EMemoryArea.UNKNOWN:
                // intentional fallthrough
                default:
                    return new SolidColorBrush(Colors.DarkBlue);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
