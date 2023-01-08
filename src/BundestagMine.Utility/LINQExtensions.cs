using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Utility
{
    public static class LINQExtensions
    {
        private static Random rng = new Random();

        public static double AverageOrDefault(this IEnumerable<double> enumerable)
        {
            return enumerable.DefaultIfEmpty().Average();
        }

        public static double AverageOrDefault<T>(this IEnumerable<T> enumerable, Func<T, double> selector)
        {
            return enumerable.Select(selector).DefaultIfEmpty().Average();
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static IEnumerable<T> LogLINQ<T>(this IEnumerable<T> enumerable, string logName, Func<T, string> printMethod)
        {
#if DEBUG
            int count = 0;
            foreach (var item in enumerable)
            {
                if (printMethod != null)
                {
                    Debug.WriteLine($"{logName}|item {count} = {printMethod(item)}");
                }
                count++;
                yield return item;
            }
            Debug.WriteLine($"{logName}|count = {count}");
#else
    return enumerable;
#endif
        }
    }
}
