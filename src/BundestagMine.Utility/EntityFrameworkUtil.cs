using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BundestagMine.Utility
{
    public static class EntityFrameworkUtil
    {
        /// <summary>
        /// Always order the query before using this method!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        public static IEnumerable<T> QueryInChunksOf<T>(this IQueryable<T> queryable, int chunkSize)
        {
            return queryable.QueryChunksOfSize(chunkSize).SelectMany(chunk => chunk);
        }

        public static IEnumerable<T[]> QueryChunksOfSize<T>(this IQueryable<T> queryable, int chunkSize)
        {
            int chunkNumber = 0;
            while (true)
            {
                var query = (chunkNumber == 0)
                    ? queryable
                    : queryable.Skip(chunkNumber * chunkSize);
                var chunk = query.Take(chunkSize).ToArray();
                if (chunk.Length == 0)
                    yield break;
                yield return chunk;
                chunkNumber++;
            }
        }
    }
}
