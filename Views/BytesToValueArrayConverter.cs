using CppMemoryVisualizer.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CppMemoryVisualizer.Views
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

            int blockSize = (int)(memory.TypeInfo.Size / totalLength);
            List<int> values = new List<int>((int)totalLength);
            for (uint i = 0; i < totalLength; ++i)
            {
                values.Add(BitConverter.ToInt32(memory.ByteValues, (int)i * blockSize));
            }

            return values;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
