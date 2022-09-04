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
        /// A list of all namedentities that are not really suitable as a topic
        /// </summary>
        public static List<string> TopicBlackList = new()
        {
            "deutsch", "Präsident !", "europäisch", "europäische", "lieben Kollegin"
        };
    }
}
