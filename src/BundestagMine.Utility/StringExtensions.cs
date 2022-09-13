using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Utility
{
    public static class StringExtensions
    {
        public static string FirstCharToLowerCase(this string? str)
        {
            if (!string.IsNullOrEmpty(str) && char.IsUpper(str[0]))
                return str.Length == 1 ? char.ToLower(str[0]).ToString() : char.ToLower(str[0]) + str[1..];

            return str;
        }

        /// <summary>
        /// Sometimes, NE start or end with ! and other satzzeichen. Clean them from them.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToCleanNE(this string? str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            if (str.StartsWith('.')) str = str.Substring(1);
            if (str.StartsWith('!')) str = str.Substring(1);
            if (str.EndsWith('!')) str = str.Substring(0, str.Length - 1);
            if (str.StartsWith('?')) str = str.Substring(1);
            if (str.EndsWith('?')) str = str.Substring(0, str.Length - 1);
            if (str.StartsWith(',')) str = str.Substring(1);
            if (str.EndsWith(',')) str = str.Substring(0, str.Length - 1);
            if (str.StartsWith('.')) str = str.Substring(1);

            str = str.Trim();
            return str;
        }
    }
}
