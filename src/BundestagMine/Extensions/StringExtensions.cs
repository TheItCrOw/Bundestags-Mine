using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.Extensions
{
    public static class StringExtensions
    {
        public static string ToCleanRequestString(this string str)
        {
            return str.Replace("{SLASH}", "/").Replace("{NULL}", "");
        }
    }
}
