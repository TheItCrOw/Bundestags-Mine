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
        private string _summaryDoesntExist => "Zusammenfassung nicht vorhanden. Dies liegt entweder daran, dass " +
            "die Rede zu kurz ist oder noch nicht von der Pipeline kalkuliert wurde.";
        public string GetAbstractSummaryPEGASUS => string.IsNullOrEmpty(AbstractSummaryPEGASUS) ? _summaryDoesntExist : AbstractSummaryPEGASUS;
        public string GetAbstractSummary => string.IsNullOrEmpty(AbstractSummary) ? _summaryDoesntExist : AbstractSummary;
        public string GetExtractiveSummary=> string.IsNullOrEmpty(ExtractiveSummary) ? _summaryDoesntExist : ExtractiveSummary;
        public string ExtractiveSummary { get; set; }
        public string EnglishTranslationOfSpeech { get; set; }
        public double EnglishTranslationScore { get; set; }
    }
}
