using CppMemoryVisualizer.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CppMemoryVisualizer.Converters
{
    // https://stackoverflow.com/questions/3652688/mutually-exclusive-checkable-menu-items/11497189#11497189

    public sealed class StandardCppVersionEnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EStandardCppVersion paramVal = (EStandardCppVersion)parameter;
            EStandardCppVersion objVal = (EStandardCppVersion)value;

            return paramVal == objVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                EStandardCppVersion selectedVersion = (EStandardCppVersion)parameter;

                if ((bool)value)
                {
                    return System.Convert.ChangeType(selectedVersion, targetType);
                }
            }

            return false;
        }
    }
}
