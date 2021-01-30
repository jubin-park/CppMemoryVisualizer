using CppMemoryVisualizer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CppMemoryVisualizer.Converters
{
    class BytesToPointerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MemoryOwnerInfo memory = value as MemoryOwnerInfo;

            Debug.Assert(memory.TypeInfo.ArrayLengths.Count <= 0);
            Debug.Assert(memory.TypeInfo.Size == 4);

            uint pointer = memory.ByteValues[0] | (uint)memory.ByteValues[1] << 8 | (uint)memory.ByteValues[2] << 16 | (uint)memory.ByteValues[3] << 24;

            if (pointer == 0)
            {
                return "(nullptr)";
            }
            else
            {
                return string.Format("0x{0:x8}", pointer);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
