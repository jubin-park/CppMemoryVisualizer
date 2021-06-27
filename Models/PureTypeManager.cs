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

                "short",
                "signed short",
                "unsigned short",
                "short int",
                "signed short int",
                "unsigned short int",

                "int",
                "signed int",
                "unsigned int",
                "long",
                "signed long",
                "unsigned long",

                "long long",
                "signed long long",
                "unsigned long long",

                "float",
                "double",

                /* const */
                "const char",
                "const signed char",
                "const unsigned char",

                "const short",
                "const signed short",
                "const unsigned short",
                "const short int",
                "const signed short int",
                "const unsigned short int",

                "const int",
                "const signed int",
                "const unsigned int",
                "const long",
                "const signed long",
                "const unsigned long",

                "const long long",
                "const signed long long",
                "const unsigned long long",

                "const float",
                "const double"
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
            //if (typeName.StartsWith("const "))
            //{
                //typeName = typeName.Substring("const ".Length);
            //}
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
