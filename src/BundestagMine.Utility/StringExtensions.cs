using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public static string ToUnescapedMail(this string? str) => str.Replace("{AT}", "@").Replace("{DOT}", ".");
        public static string ToEscapedMail(this string? str) => str.Replace("@", "{AT}").Replace(".", "{DOT}");

        public static string StripHTML(this string? str)
        {
            return Regex.Replace(str, "<.*?>", string.Empty);
        }

        public static string StripTabs(this string? str)
        {
            return Regex.Replace(str, @"\s+", " ");
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
