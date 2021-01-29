using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Models
{
    sealed class HeapManager
    {
        private readonly List<ulong> mHeapKeys = new List<ulong>(32); // (ulong)heapSize << 32 | heapAddress;
        private Dictionary<ulong, HeapMemoryInfo> mCaches = new Dictionary<ulong, HeapMemoryInfo>();

        public void Add(ulong key)
        {
            mHeapKeys.Add(key);
        }

        public void Clear()
        {
            mHeapKeys.Clear();
        }

        public void Update()
        {
            List<ulong> existedKeys = new List<ulong>();
            foreach (ulong key in mHeapKeys)
            {
                if (mCaches.ContainsKey(key))
                {
                    existedKeys.Add(key);
                }
            }

            // subtract existedKeys from all keys
            List<ulong> removalKeys = new List<ulong>(mCaches.Keys);
            foreach (ulong key in existedKeys)
            {
                removalKeys.Remove(key);
            }
            foreach (ulong key in removalKeys)
            {
                mCaches.Remove(key);
            }

            foreach (ulong key in mHeapKeys)
            {
                if (!mCaches.ContainsKey(key))
                {
                    mCaches.Add(key, new HeapMemoryInfo((uint)(key & uint.MaxValue), (uint)(key >> 32)));
                }
            }
        }

        public HeapMemoryInfo GetHeapOrNull(uint heapAddress)
        {
            if (mHeapKeys.Count == 0)
            {
                return null;
            }
            
            int left = 0;
            int right = mHeapKeys.Count - 1;

            while (left <= right)
            {
                int mid = (left + right) / 2;

                uint nowAddress = (uint)(mHeapKeys[mid] & uint.MaxValue);
                uint nowSize = (uint)(mHeapKeys[mid] >> 32);

                if (nowAddress <= heapAddress && heapAddress < nowAddress + nowSize)
                {
                    HeapMemoryInfo heap = null;
                    bool bSuccess = mCaches.TryGetValue(mHeapKeys[mid], out heap);
                    Debug.Assert(bSuccess);

                    return heap;
                }
                else if (heapAddress < nowAddress)
                {
                    right = mid - 1;
                }
                else if (heapAddress > nowAddress + nowSize)
                {
                    left = mid + 1;
                }
                else
                {
                    Debug.Assert(false, "invalid range");
                }
            }

            return null;
        }

        public void SetAllInvisible()
        {
            foreach (var heap in mCaches.Values)
            {
                heap.IsVisible = false;
            }
        }
    }
}
