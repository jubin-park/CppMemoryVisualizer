using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Models
{
    sealed class PureTypeManager
    {
        private Dictionary<string, TypeInfo> mTypes = new Dictionary<string, TypeInfo>();

        /*
        public PureTypeManager()
        {
            {
                var typeChar = new TypeInfo()
                {
                    PureName = "char",
                    Size = 1
                };
                mTypes.Add("char", typeChar);
                mTypes.Add("Char", typeChar);
            }

            {
                var typeShort = new TypeInfo()
                {
                    PureName = "short",
                    Size = 2
                };
                mTypes.Add("short", typeShort);
                mTypes.Add("Int2B", typeShort);
            }

            {
                var typeInt = new TypeInfo()
                {
                    PureName = "int",
                    Size = 4
                };
                mTypes.Add("int", typeInt);
                mTypes.Add("Int4B", typeInt);
                mTypes.Add("long", typeInt);
            }

            {
                var typeUChar = new TypeInfo()
                {
                    PureName = "unsigned char",
                    Size = 1
                };
                mTypes.Add("unsigned char", typeUChar);
                mTypes.Add("UChar", typeUChar);
            }

            {
                var typeUShort = new TypeInfo()
                {
                    PureName = "unsigned short",
                    Size = 2
                };
                mTypes.Add("unsigned short", typeUShort);
                mTypes.Add("UInt2B", typeUShort);
            }

            {
                var typeUInt = new TypeInfo()
                {
                    PureName = "unsigned int",
                    Size = 4
                };
                mTypes.Add("unsigned int", typeUInt);
                mTypes.Add("UInt4B", typeUInt);
                mTypes.Add("unsigned long", typeUInt);
            }

            {
                var typeFloat = new TypeInfo()
                {
                    PureName = "float",
                    Size = 4
                };
                mTypes.Add("float", typeFloat);
                mTypes.Add("Float", typeFloat);
            }

            {
                var typeDouble = new TypeInfo()
                {
                    PureName = "double",
                    Size = 8
                };
                mTypes.Add("double", typeDouble);
                mTypes.Add("Double", typeDouble);
            }

            {
                var typePointer = new TypeInfo()
                {
                    PureName = "pointer",
                    Size = 4
                };
                mTypes.Add("<function>", typePointer);
                mTypes.Add("Ptr32", typePointer);
            }
        }
        */

        public void AddType(string typeName, TypeInfo pure)
        {
            Debug.Assert(typeName != null);
            Debug.Assert(pure != null);

            mTypes.Add(typeName, pure);
        }

        public bool HasType(string typeName)
        {
            Debug.Assert(typeName != null);

            return mTypes.ContainsKey(typeName);
        }

        public TypeInfo GetType(string typeName)
        {
            Debug.Assert(typeName != null);

            TypeInfo pure = null;
            Debug.Assert(mTypes.TryGetValue(typeName, out pure));

            return pure;
        }
    }
}
