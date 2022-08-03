using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.Extensions
{
    public static class LINQExtensions
    {
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
