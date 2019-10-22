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
    public static class Algoritm
    {
        public interface IIndexer<Tval>
        {
            Tval this[int index] { get; set; }
            int Length { get; }
        }

        public static void QuickSort<Tarray, Tval>(ref Tarray sortArray, bool descending = false) where Tarray : IIndexer<Tval> where Tval : IComparable<Tval>
        {
            QSort<Tarray, Tval>(ref sortArray, 0, sortArray.Length - 1, descending);
        }

        static void QSort<Tarray, Tval>(ref Tarray arr, int low, int high, bool descending = false) where Tarray : IIndexer<Tval> where Tval : IComparable<Tval>
        {
            if (low < high)
            {

                var pi = PartitionLomuto<Tarray, Tval>(ref arr, low, high, descending);
                //var pi = PartitionHoar(ref arr, low, high);
                QSort<Tarray, Tval>(ref arr, low, pi - 1, descending);
                QSort<Tarray, Tval>(ref arr, pi + 1, high, descending);
            }
        }

        /// <summary>
        /// Разбиение Ломуто
        /// </summary>
        /// <returns></returns>
        static int PartitionLomuto<Tarray, Tval>(ref Tarray arr, int low, int high, bool descending) where Tarray : IIndexer<Tval> where Tval : IComparable<Tval>
        {
            Tval pivot = arr[high];
            int desc = 1;
            if (descending) desc = -1;

            int i = low - 1;
            for (int j = low; j <= high - 1; j++)
            {
                if (arr[j].CompareTo(pivot) * desc <= 0)
                {
                    i++;
                    Swap<Tarray, Tval>(ref arr, i, j);
                }
            }
            Swap<Tarray, Tval>(ref arr, i + 1, high);
            return i + 1;
        }

        static void Swap<Tarray, Tval>(ref Tarray arr, int i, int j) where Tarray : IIndexer<Tval> where Tval : IComparable<Tval>
        {
            Tval temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }
    }
}
