using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.RequestModels
{
    public class GlobalSearchRequest
    {
        public string SearchString { get; set; }
        public bool IncludeSpeeches { get; set; }
        public bool IncludeSpeakers { get; set; }
        public bool IncludeAgendaItems { get; set; }
        public bool IncludePolls { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
