using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.RequestModels
{
    public class ReportPageRequest
    {
        public int PageNumber { get; set; }
        public string ReportId { get; set; }
        public string SpeakerId { get; set; }
        public string Fraction { get; set; }
        public string Party { get; set; }

        // NO IDEA WHY, but we cant have the From and To date as DateTime. Otherwise the validation doesnt work
        // although it works everywhere else.
        public string From { get; set; }
        public string To { get; set; }
        public string Topic { get; set; }
    }
}
