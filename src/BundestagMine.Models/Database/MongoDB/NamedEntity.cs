using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.Models.Database.MongoDB
{
    public class NamedEntity : DBEntity
    {
        public string LemmaValue { get; set; }
        public Guid NLPSpeechId { get; set; }
        public Guid ShoutId { get; set; }
        public int Begin { get; set; }
        public int End { get; set; }
        public string Value { get; set; }
    }
}
