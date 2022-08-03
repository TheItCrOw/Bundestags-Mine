using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.Models.Database.MongoDB
{
    public class NLPSpeech : Speech
    {
        public List<CategoryCoveredTagged> CategoryCoveredTags { get; set; }
        public List<NamedEntity> NamedEntities { get; set; }
        public List<Token> Tokens { get; set; }
        public List<Sentiment> Sentiments { get; set; }
    }
}
