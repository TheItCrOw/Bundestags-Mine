using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.Utility
{
    public static class NameHelper
    {
        /// <summary>
        /// The protocols and polls of the bundestag often have slighlyt different names for the fractions and parties...
        /// This is a accumulation of those pemutations.
        /// </summary>
        /// <param name="org"></param>
        /// <returns></returns>
        public static List<string> GetAliasesOf(string org)
        {
            if (org.ToLower().Contains("die linke"))
                return new List<string>()
                {
                    "DIE LINKE.", "DIE LINKE", "Die Linke"
                };
            else if (org.ToLower().Contains("ndnis"))
                return new List<string>()
                {
                    "Bündnis 90 / Die Grünen", "BÜNDNIS 90/DIE GRÜNEN", "BÜNDNIS`90/DIE GRÜNEN",
                    "BÜ90/GR"
                };
            return new List<string>() { org };
        }
    }
}
