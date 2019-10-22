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
    }
}
