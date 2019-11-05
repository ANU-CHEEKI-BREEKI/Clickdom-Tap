using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ANU.Utils
{
    public static class Jobs
    {
        [BurstCompile]
        public struct QuickSortRecursivelyJob<T> : IJob where T : struct, IComparable<T>
        {
            public NativeArray<T> sortArray;
            [ReadOnly] public bool descending;

            public void Execute()
            {
                var indexer = new NativeArrayIndexer<T>(sortArray);

                Algoritm.QuickSort<NativeArrayIndexer<T>, T>(ref indexer, descending);
            }
        }

        [BurstCompile]
        public struct MultiHashToQueueJob<TKey, TVal> : IJob where TKey : struct, IEquatable<TKey> where TVal : struct
        {
            public NativeQueue<TVal> queue;
            [ReadOnly] public NativeMultiHashMap<TKey, TVal> map;
            [ReadOnly] public TKey key;

            public void Execute()
            {
                TVal rdata;
                NativeMultiHashMapIterator<TKey> iterator;
                if (map.TryGetFirstValue(key, out rdata, out iterator))
                {
                    queue.Enqueue(rdata);
                    while (map.TryGetNextValue(out rdata, ref iterator))
                        queue.Enqueue(rdata);
                }
            }
        }

        [BurstCompile]
        public struct QueueToArrayJob<T> : IJob where T : struct
        {
            public NativeArray<T> array;
            public NativeQueue<T> queue;

            public void Execute()
            {
                int index = 0;
                int arrayLength = array.Length;
                T rdata;
                while (index < arrayLength && queue.TryDequeue(out rdata))
                {
                    array[index] = rdata;
                    index++;
                }
            }
        }
    }
}
