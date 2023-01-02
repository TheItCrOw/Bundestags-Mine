using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Utility
{
    public static class TopicHelper
    {
        /// <summary>
        /// A list of all namedentities that are not really suitable as a topic.
        /// This is of course very hardcody. It would be nice to detect them programmaticaly some day.
        /// </summary>
        public static List<string> TopicBlackList = new()
        {
            "deutsch", "Präsident !", "europäisch", "europäische", "lieben Kollegin", "...", "…", "Kollegin lieben",
            "Erstens", "Zweitens", "Liebe Kolleginnen", "sich", "lieben Kollege", "umso", "Na ja", "Herzliche", "ehrlich",
            "viel", "’s", "sein", " Kollegin lieben", "deutschen", "deutsche"
        };
    }
}
