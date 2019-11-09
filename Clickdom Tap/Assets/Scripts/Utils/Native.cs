using ANU.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;

namespace ANU.Utils
{
    public static class NativeUtils
    {
        public static void IterateForKey<Tkey, Tval>(this NativeMultiHashMap<Tkey, Tval> map, Tkey key, Action<Tval> action) where Tkey : struct, IEquatable<Tkey> where Tval : struct
        {
            NativeMultiHashMapIterator<Tkey> iterator;
            Tval val;

            if (map.TryGetFirstValue(key, out val, out iterator))
            {
                do
                {
                    action.Invoke(val);
                }
                while (map.TryGetNextValue(out val, ref iterator));
            }
        }

        public static void IterateForKey<Tkey, Tval>(this NativeMultiHashMap<Tkey, Tval> map, Tkey key, Func<Tval, bool> action) where Tkey : struct, IEquatable<Tkey> where Tval : struct
        {
            NativeMultiHashMapIterator<Tkey> iterator;
            Tval val;

            if (map.TryGetFirstValue(key, out val, out iterator))
            {
                do
                {
                    if (!action.Invoke(val))
                        return;
                }
                while (map.TryGetNextValue(out val, ref iterator));
            }
        }

        public static int CountForKey<Tkey, Tval>(this NativeMultiHashMap<Tkey, Tval> map, Tkey key) where Tkey : struct, IEquatable<Tkey> where Tval : struct
        {
            NativeMultiHashMapIterator<Tkey> iterator;
            Tval value;
            var cnt = 0;

            if (map.TryGetFirstValue(key, out value, out iterator))
            {
                do
                {
                    cnt++;
                }
                while (map.TryGetNextValue(out value, ref iterator));
            }

            return cnt;
        }

        public static bool ContainsValueForKey<Tkey, Tval>(this NativeMultiHashMap<Tkey, Tval> map, Tkey key, Tval val) where Tkey : struct, IEquatable<Tkey> where Tval : struct, IEquatable<Tval>
        {
            NativeMultiHashMapIterator<Tkey> iterator;
            Tval value;
            var contains = false;

            if (map.TryGetFirstValue(key, out value, out iterator))
            {
                do
                {
                    contains = value.Equals(val);
                    if (contains)
                        break;
                }
                while (map.TryGetNextValue(out value, ref iterator));
            }

            return contains;
        }

        public static NativeArray<Tkey> GetUniqueKeys<Tkey, Tval>(this NativeMultiHashMap<Tkey, Tval> map, Allocator alocator) where Tkey : struct, IEquatable<Tkey>, IComparable<Tkey> where Tval : struct
        {
            var tempKeys = map.GetKeyArray(Allocator.Temp);
            var tempMap = new NativeHashMap<Tkey, Tkey>(tempKeys.Length, Allocator.Temp);

            for (int i = 0; i < tempKeys.Length; i++)
                tempMap.TryAdd(tempKeys[i], tempKeys[i]);
            var res = tempMap.GetKeyArray(alocator);

            tempKeys.Dispose();
            tempMap.Dispose();

            return res;
        }

        public static bool ContainsValue<Tval>(this NativeArray<Tval> array, Tval value) where Tval : struct, IEquatable<Tval>
        {
            for (int i = 0; i < array.Length; i++)
                if (array[i].Equals(value))
                    return true;
            return false;
        }

        public static bool ContainsValue<Tval>(this NativeList<Tval> list, Tval value) where Tval : struct, IEquatable<Tval>
        {
            for (int i = 0; i < list.Length; i++)
                if (list[i].Equals(value))
                    return true;
            return false;
        }

        public static JobHandle GetUniqueKeysDependsOn<Tkey, Tval>(
            this NativeMultiHashMap<Tkey, Tval> map, 
            out NativeQueue<Tkey> buffer,
            out NativeList<Tkey> uniqueKeys, 
            Allocator allocator, 
            JobHandle inputDeps
        ) 
            where Tkey : struct, IEquatable<Tkey>, IComparable<Tkey> 
            where Tval : struct
        {
            buffer = new NativeQueue<Tkey>(allocator);
            uniqueKeys = new NativeList<Tkey>(allocator);

            var deps = new Jobs.MultiHashToKeysQueueJob<Tkey, Tval>()
            {
                keys = buffer.AsParallelWriter()
            }.Schedule(map, 1, inputDeps);
            var resHandle = new Jobs.QueueToUniqueListValuesJob<Tkey>()
            {
                vals = buffer,
                uniqueVals = uniqueKeys
            }.Schedule(deps);
            return resHandle;
        }
    }
}