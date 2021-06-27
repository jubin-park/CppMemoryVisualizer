using CppMemoryVisualizer.Enums;
using CppMemoryVisualizer.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace CppMemoryVisualizer.Converters
{
    class ArraySegmentToValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (null == values[0] || null == values[1])
            {
                return null;
            }

            var type = new TypeInfo();
            type.SetByString((string)values[0]);

            var segment = (ArraySegment<byte>)values[1];
            byte[] bytes = new byte[segment.Count];
            for (uint i = 0; i < segment.Count; ++i)
            {
                bytes[i] = segment.Array[segment.Offset + i];
            }

            string str = null;

            if ((type.Flags & (EMemoryTypeFlags.POINTER | EMemoryTypeFlags.ARRAY_OR_FUNCTION_POINTER)) != EMemoryTypeFlags.NONE)
            {
                try
                {
                    uint pointer = BitConverter.ToUInt32(bytes, 0);
                    str = (0 == pointer ? "nullptr" : string.Format("0x{0:x8}", pointer));
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine(type.FullNameOrNull);

                    return null;
                }
            }
            else
            {
                switch (type.PureName)
                {
                    case "char":
                    /* intentional fallthrough */
                    case "const char":
                    /* intentional fallthrough */
                    case "signed char":
                    /* intentional fallthrough */
                    case "const signed char":
                        if (0 == bytes[0])
                        {
                            str = string.Format("'{0}' ({1})", "\\0", (sbyte)bytes[0]);
                        }
                        else
                        {
                            str = string.Format("'{0}' ({1})", (char)bytes[0], (sbyte)bytes[0]);
                        }
                        break;

                    case "unsigned char":
                    /* intentional fallthrough */
                    case "const unsigned char":
                        if (0 == bytes[0])
                        {
                            str = string.Format("'{0}' ({1})", "\\0", bytes[0]);
                        }
                        else
                        {
                            str = string.Format("'{0}' ({1})", (char)bytes[0], bytes[0]);
                        }
                        
                        break;

                    case "short":
                    /* intentional fallthrough */
                    case "const short":
                    /* intentional fallthrough */
                    case "signed short":
                    /* intentional fallthrough */
                    case "const signed short":
                    /* intentional fallthrough */

                    case "short int":
                    /* intentional fallthrough */
                    case "const short int":
                    /* intentional fallthrough */
                    case "signed short int":
                    /* intentional fallthrough */
                    case "const signed short int":
                        {
                            short val = BitConverter.ToInt16(bytes, 0);
                            str = val.ToString();
                        }
                        break;

                    case "unsigned short":
                    /* intentional fallthrough */
                    case "const unsigned short":
                    /* intentional fallthrough */

                    case "unsigned short int":
                    /* intentional fallthrough */
                    case "const unsigned short int":
                        {
                            ushort val = BitConverter.ToUInt16(bytes, 0);
                            str = val.ToString();
                        }
                        break;

                    case "int":
                    /* intentional fallthrough */
                    case "const int":
                    /* intentional fallthrough */
                    case "signed int":
                    /* intentional fallthrough */
                    case "const signed int":
                    /* intentional fallthrough */

                    case "long":
                    /* intentional fallthrough */
                    case "const long":
                    /* intentional fallthrough */
                    case "signed long":
                    /* intentional fallthrough */
                    case "const signed long":
                        {
                            int val = BitConverter.ToInt32(bytes, 0);
                            str = val.ToString();
                        }
                        break;

                    case "const unsigned int":
                    /* intentional fallthrough */
                    case "unsigned int":
                        {
                            uint val = BitConverter.ToUInt32(bytes, 0);
                            str = val.ToString();
                        }
                        break;

                    case "long long":
                    /* intentional fallthrough */
                    case "const long long":
                    /* intentional fallthrough */
                    case "signed long long":
                    /* intentional fallthrough */
                    case "const signed long long":
                        {
                            long val = BitConverter.ToInt64(bytes, 0);
                            str = val.ToString();
                        }
                        break;

                    case "unsigned long long":
                    /* intentional fallthrough */
                    case "const unsigned long long":
                        {
                            ulong val = BitConverter.ToUInt64(bytes, 0);
                            str = val.ToString();
                        }
                        break;

                    case "float":
                    /* intentional fallthrough */
                    case "const float":
                        {
                            float val = BitConverter.ToSingle(bytes, 0);
                            str = val.ToString();
                        }
                        break;

                    case "double":
                    /* intentional fallthrough */
                    case "const double":
                        {
                            double val = BitConverter.ToDouble(bytes, 0);
                            str = val.ToString();
                        }
                        break;

                    default:
                        break;
                }
            }

            return str;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
