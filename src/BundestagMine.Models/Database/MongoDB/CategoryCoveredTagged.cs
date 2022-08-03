using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.Models.Database.MongoDB
{
    public class CategoryCoveredTagged : DBEntity
    {
        public int Begin { get; set; }
        public int End { get; set; }
        public string Value { get; set; }
        public double Score { get; set; }
        public Guid NLPSpeechId { get; set; }
    }
}
