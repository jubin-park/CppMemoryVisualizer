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
    class BytesToValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MemoryOwnerInfo memory = value as MemoryOwnerInfo;

            Debug.Assert(memory.TypeInfo.ArrayLengths.Count <= 0);
            Debug.Assert(memory.TypeInfo.Size <= 8);

            if (memory.TypeInfo.PureName == "float")
            {
                return BitConverter.ToSingle(memory.ByteValues, 0);
            }
            else if (memory.TypeInfo.PureName == "double")
            {
                return BitConverter.ToDouble(memory.ByteValues, 0);
            }
            else if (memory.TypeInfo.PureName.StartsWith("unsigned "))
            {
                switch (memory.TypeInfo.Size)
                {
                    case 1:
                        return memory.ByteValues[0];

                    case 2:
                        return memory.ByteValues[0] | memory.ByteValues[1] << 8;

                    case 4:
                        return memory.ByteValues[0] | (uint)memory.ByteValues[1] << 8 | (uint)memory.ByteValues[2] << 16 | (uint)memory.ByteValues[3] << 24;

                    case 8:
                        // intentional fallthrough
                    default:
                        Debug.Assert(false, "Invalid size");
                        break;
                }
            }
            else
            {
                switch (memory.TypeInfo.Size)
                {
                    case 1:
                        return System.Convert.ToSByte(memory.ByteValues);

                    case 2:
                        return BitConverter.ToInt16(memory.ByteValues, 0);

                    case 4:
                        return BitConverter.ToInt32(memory.ByteValues, 0);

                    case 8:
                        // intentional fallthrough
                    default:
                        Debug.Assert(false, "Invalid size");
                        break;
                }
            }

            return null;

            /*
            uint val = 0;

            for (int i = (int)(memory.TypeInfo.Size / 4) * 4 - 1; i > 0; i -= 4)
            {
                val += memory.ByteValues[i] + ((uint)memory.ByteValues[i - 1] << 8) + ((uint)memory.ByteValues[i - 2] << 16) + ((uint)memory.ByteValues[i - 3] << 24);
            }

            for (int i = (int)memory.TypeInfo.Size % 4; i >= 0; --i)
            {
                val <<= 8;
                val += memory.ByteValues[i];
            }

            return val;
            */
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
