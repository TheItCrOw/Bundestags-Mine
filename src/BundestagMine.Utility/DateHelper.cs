using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace BundestagMine.Utility
{
    public class DateHelper
    {

        /// <summary>
        /// Convert '6. April 2022' to regular date
        /// </summary>
        /// <returns></returns>
        public static string GermanStringDateToStringDate(string date)
        {
            var correctDate = DateTime.Parse(date).ToString("yyyy/MM/dd").Replace(".", "-");
            return correctDate;
        }

        public static string RemoveWhitespaces(string str)
            => string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));

        /// <summary>
        /// Replaces invalid characters for a windows filepath
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string ReplaceInvalidPathChars(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        public static string ConvertGermanUmlauteBack(string t)
        {
            t = t.Replace("ae", "ä");
            t = t.Replace("oe", "ä");
            t = t.Replace("ue", "ü");
            t = t.Replace("Ae", "Ä");
            t = t.Replace("Oe", "Ö");
            t = t.Replace("Ue", "Ü");
            //t = t.Replace("ss", "ß");
            return t;
        }
    }
}
