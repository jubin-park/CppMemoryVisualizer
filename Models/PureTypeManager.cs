using System.Collections.Generic;
using System.Diagnostics;

namespace CppMemoryVisualizer.Models
{
    static class PureTypeManager
    {
        private static Dictionary<string, TypeInfo> mTypes = new Dictionary<string, TypeInfo>();

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
