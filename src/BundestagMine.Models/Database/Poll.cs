using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.Models.Database
{
    public class Poll : DBEntity
    {
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public int LegislaturePeriod { get; set; }
        public int ProtocolNumber { get; set; }
        public int PollNumber { get; set; }
        public List<PollEntry> Entries { get; set; }
    }
}
