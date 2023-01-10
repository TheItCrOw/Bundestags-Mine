using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Models.Database
{
    public class DailyPaper : DBEntity
    {
        public int LegislaturePeriod { get; set; }
        public int ProtocolNumber { get; set; }
        public DateTime ProtocolDate { get; set; }
        public DateTime Created { get; set; }

        /// <summary>
        /// This is the dailypaper paper serialized as json
        /// </summary>
        public string JsonDataString { get; set; }
    }
}
