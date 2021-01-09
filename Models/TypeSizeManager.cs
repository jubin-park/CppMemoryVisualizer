using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Models
{
    sealed class TypeSizeManager
    {
        private Dictionary<string, uint> mByteSizes = new Dictionary<string, uint>()
        {
            { "char", 1 },
            { "short", 2 },
            { "int", 4 },
            { "long", 4 },
            { "float", 4 },
            { "double", 8 },
            { "unsigned char", 1 },
            { "unsigned short", 2 },
            { "unsigned int", 4 },
            { "unsigned long", 4 },
            { "<function>", 4 },
        };

        public void Add(string typeName, uint size)
        {
            Debug.Assert(typeName != null);

            mByteSizes.Add(typeName, size);
        }

        public bool HasSize(string typeName)
        {
            Debug.Assert(typeName != null);

            return mByteSizes.ContainsKey(typeName);
        }

        public uint GetSize(string typeName)
        {
            Debug.Assert(typeName != null);

            uint size = 0;
            Debug.Assert(mByteSizes.TryGetValue(typeName, out size));

            return size;
        }
    }
}
