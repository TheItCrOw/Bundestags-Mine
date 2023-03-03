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

        /// <summary>
        /// This is the abstract summary of the <see cref="Text"/> done by BART
        /// </summary>
        public string AbstractSummary { get; set; }

        /// <summary>
        /// This is the abstract summary of the <see cref="Text"/> done by PEGASUS
        /// </summary>
        public string AbstractSummaryPEGASUS { get; set; }
        public string ExtractiveSummary { get; set; }
        public string EnglishTranslationOfSpeech { get; set; }
        public double EnglishTranslationScore { get; set; }
    }
}
