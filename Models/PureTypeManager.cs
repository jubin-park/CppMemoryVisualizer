using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CppMemoryVisualizer.Models
{
    static class PureTypeManager
    {
        private static Dictionary<string, TypeInfo> mTypes = new Dictionary<string, TypeInfo>();

        public static readonly ReadOnlyCollection<string> PRIMITIVE_TYPE_NAMES = new ReadOnlyCollection<string>(
            new string[]
            {
                "char",
                "signed char",
                "unsigned char",
                "int8_t",
                "uint8_t",

                "short",
                "signed short",
                "unsigned short",
                "short int",
                "signed short int",
                "unsigned short int",
                "int16_t",
                "uint16_t",

                "int",
                "signed int",
                "unsigned int",
                "int32_t",
                "uint32_t",

                "long",
                "signed long",
                "unsigned long",

                "long long",
                "signed long long",
                "unsigned long long",
                "int64_t",
                "uint64_t",

                "float",
                "double"
            }
        );

        public static void AddType(string typeName, TypeInfo pure)
        {
            Debug.Assert(null != typeName);
            Debug.Assert(null != pure);

            mTypes.Add(typeName, pure);
        }

        public static bool HasType(string typeName)
        {
            Debug.Assert(null != typeName);

            return mTypes.ContainsKey(typeName);
        }

        public static TypeInfo GetType(string typeName)
        {
            Debug.Assert(null != typeName);

            TypeInfo pure = null;
            if (typeName.StartsWith("const "))
            {
                typeName = typeName.Substring("const ".Length);
            }
            bool bSuccess = mTypes.TryGetValue(typeName, out pure);
            Debug.Assert(bSuccess);

            return pure;
        }

        public static void Clear()
        {
            mTypes.Clear();
        }
    }
}
