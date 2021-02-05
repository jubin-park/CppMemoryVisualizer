﻿using CppMemoryVisualizer.Enums;
using CppMemoryVisualizer.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CppMemoryVisualizer.Converters
{
    class ArraySegmentsToValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null || values[1] == null)
            {
                return null;
            }

            var type = new TypeInfo() { FullNameOrNull = (string)values[0] };

            var segment = (ArraySegment<byte>)values[1];
            byte[] bytes = new byte[segment.Count];
            for (uint i = 0; i < segment.Count; ++i)
            {
                bytes[i] = segment.Array[segment.Offset + i];
            }

            string str = null;

            if ((type.Flags & (EMemoryTypeFlags.POINTER | EMemoryTypeFlags.ARRAY_OR_FUNCTION_POINTER)) != EMemoryTypeFlags.NONE)
            {
                uint pointer = BitConverter.ToUInt32(bytes, 0);
                str = (pointer == 0 ? "nullptr" : string.Format("0x{0:x8}", pointer));
            }
            else
            {
                switch (type.PureName)
                {
                    case "char":
                    case "int8_t":
                        str = string.Format("'{0}' ({1})", (char)bytes[0], (sbyte)bytes[0]);
                        break;

                    case "unsigned char":
                    case "uint8_t":
                        str = string.Format("'{0}' ({1})", (char)bytes[0], bytes[0]);
                        break;

                    case "short":
                    case "int16_t":
                        {
                            short val = BitConverter.ToInt16(bytes, 0);
                            str = val.ToString();
                        }
                        break;

                    case "unsigned short":
                    case "uint16_t":
                        {
                            ushort val = BitConverter.ToUInt16(bytes, 0);
                            str = val.ToString();
                        }
                        break;

                    case "int":
                    case "int32_t":
                        {
                            int val = BitConverter.ToInt32(bytes, 0);
                            str = val.ToString();
                        }
                        break;

                    case "unsigned int":
                    case "uint32_t":
                    case "size_t":
                        {
                            uint val = BitConverter.ToUInt32(bytes, 0);
                            str = val.ToString();
                        }
                        break;

                    case "float":
                        {
                            float val = BitConverter.ToSingle(bytes, 0);
                            str = val.ToString();
                        }
                        break;

                    case "double":
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