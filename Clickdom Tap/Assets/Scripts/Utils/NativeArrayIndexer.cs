using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;

namespace ANU.Utils
{
    public struct NativeArrayIndexer<T> : Algoritm.IIndexer<T> where T : struct
    {
        private NativeArray<T> array;

        public NativeArrayIndexer(NativeArray<T> array)
        {
            this.array = array;
        }

        public T this[int index]
        {
            get => array[index];
            set => array[index] = value;
        }

        public int Length => array.Length;
    }
}
