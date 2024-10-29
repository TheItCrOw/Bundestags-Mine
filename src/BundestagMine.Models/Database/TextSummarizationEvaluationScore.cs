using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Models.Database
{
    public class TextSummarizationEvaluationScore : DBEntity
    {
        public Guid SpeechId { get; set; }
        public TextSummarizationMethods TextSummarizationMethod { get; set; }
        public double NamedEntityDistance { get; set; }
        public string LevenstheinSimilaritiesOfSentences { get; set; }
        public int AverageWordsPerSentence { get; set; }
        public double SummaryCompressionRate { get; set; }
        /// <summary>
        /// A string that explains in words why the score is what it is
        /// </summary>
        public string ScoreExplanation { get; set; }

        /// <summary>
        /// A score that determines how good the summary was. 10 is best, 0 is the worst.
        /// </summary>
        public int SummaryScore { get; set; }

        public DateTime Created { get; set; }
    }

    public enum TextSummarizationMethods
    {
        TextRank,
        BARTSamSum,
        PEGASUSSamSum
    }
}
