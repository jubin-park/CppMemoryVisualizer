using System;
using System.Globalization;
using System.Windows.Data;

namespace CppMemoryVisualizer.Converters
{
    class UIntToHexadecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            uint dec = (uint)value;

            return string.Format("0x{0:x8}", dec);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
