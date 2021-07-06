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
                "bool",                 "const bool",

                "char",                 "const char",
                "signed char",          "const signed char",

                "unsigned char",        "const unsigned char",

                "short",                "const short",
                "unsigned short",       "const unsigned short",

                "int",                  "const int",
                "long",                 "const long",

                "unsigned int",         "const unsigned int",
                "unsigned long",        "const unsigned long",

                "long long",            "const long long",
                "unsigned long long",   "const unsigned long long",

                "float",                "const float",
                "double",               "const double",
                "long double",          "const long double",

                "wchar_t",              "const wchar_t",
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
