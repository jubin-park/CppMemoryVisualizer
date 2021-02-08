using System.Collections.Generic;
using System.Diagnostics;

namespace CppMemoryVisualizer.Models
{
    static class PureTypeManager
    {
        private static Dictionary<string, TypeInfo> mTypes = new Dictionary<string, TypeInfo>();

        public static void AddType(string typeName, TypeInfo pure)
        {
            Debug.Assert(typeName != null);
            Debug.Assert(pure != null);

            mTypes.Add(typeName, pure);
        }

        public static bool HasType(string typeName)
        {
            Debug.Assert(typeName != null);

            return mTypes.ContainsKey(typeName);
        }

        public static TypeInfo GetType(string typeName)
        {
            Debug.Assert(typeName != null);

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
