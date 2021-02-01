﻿using CppMemoryVisualizer.Models;
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
    class BytesToValueArrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MemoryOwnerInfo memory = value as MemoryOwnerInfo;

            uint totalLength = 1;
            foreach (uint len in memory.TypeInfo.ArrayLengths)
            {
                totalLength *= len;
            }
            uint blockSize = memory.TypeInfo.Size / totalLength;
            List<string> values = new List<string>((int)totalLength);

            if (memory.TypeInfo.PointerLevel > 0 || memory.TypeInfo.ArrayOrFunctionPointerLevels.Count > 0)
            {
                Debug.Assert(blockSize == 4);

                for (uint i = 0; i < totalLength; ++i)
                {
                    uint pointer = BitConverter.ToUInt32(memory.ByteValues, (int)(i * blockSize));
                    string str = (pointer == 0 ? "nullptr" : string.Format("0x{0:x8}", pointer));
                    values.Add(str);
                }

                return values;
            }

            switch (memory.TypeInfo.PureName)
            {
                case "char":
                case "int8_t":
                    for (uint i = 0; i < totalLength; ++i)
                    {
                        string str = string.Format("'{0}' ({1})", (char)memory.ByteValues[i], (sbyte)memory.ByteValues[i]);
                        values.Add(str);
                    }
                    break;

                case "unsigned char":
                case "uint8_t":
                    for (uint i = 0; i < totalLength; ++i)
                    {
                        string str = string.Format("'{0}' ({1})", (char)memory.ByteValues[i], memory.ByteValues[i]);
                        values.Add(str);
                    }
                    break;

                case "short":
                case "int16_t":
                    for (uint i = 0; i < totalLength; ++i)
                    {
                        short val = BitConverter.ToInt16(memory.ByteValues, (int)(i * blockSize));
                        string str = val.ToString();
                        values.Add(str);
                    }
                    break;

                case "unsigned short":
                case "uint16_t":
                    for (uint i = 0; i < totalLength; ++i)
                    {
                        ushort val = BitConverter.ToUInt16(memory.ByteValues, (int)(i * blockSize));
                        string str = val.ToString();
                        values.Add(str);
                    }
                    break;

                case "int":
                case "int32_t":
                    for (uint i = 0; i < totalLength; ++i)
                    {
                        int val = BitConverter.ToInt32(memory.ByteValues, (int)(i * blockSize));
                        string str = val.ToString();
                        values.Add(str);
                    }
                    break;

                case "unsigned int":
                case "uint32_t":
                case "size_t":
                    for (uint i = 0; i < totalLength; ++i)
                    {
                        uint val = BitConverter.ToUInt32(memory.ByteValues, (int)(i * blockSize));
                        string str = val.ToString();
                        values.Add(str);
                    }
                    break;

                case "float":
                    for (uint i = 0; i < totalLength; ++i)
                    {
                        float val = BitConverter.ToSingle(memory.ByteValues, (int)(i * blockSize));
                        string str = val.ToString();
                        values.Add(str);
                    }
                    break;

                case "double":
                    for (uint i = 0; i < totalLength; ++i)
                    {
                        double val = BitConverter.ToDouble(memory.ByteValues, (int)(i * blockSize));
                        string str = val.ToString();
                        values.Add(str);
                    }
                    break;

                default:
                    break;
            }

            return values;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}