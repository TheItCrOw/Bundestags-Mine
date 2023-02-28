using BundestagMine.Models.Database;
using BundestagMine.Models.Database.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BundestagMine.Logic.Services
{
    public class TextSummarizationService
    {
        private readonly ILogger<TextSummarizationEvaluationScore> _logger;
        private readonly AnnotationService _annotationService;
        private readonly BundestagMineDbContext _db;

        public TextSummarizationService(BundestagMineDbContext db, 
            AnnotationService annotationService,
            ILogger<TextSummarizationEvaluationScore> logger)
        {
            _logger = logger;
            _annotationService = annotationService;
            _db = db;
        }

        public List<TextSummarizationEvaluationScore> GetEvaluationsOfSpeech(Guid speechId) =>
            _db.TextSummarizationEvaluationScores.Where(s => s.SpeechId == speechId).ToList();

        /// <summary>
        /// Starts the process of evaluating all automatic text summaries of all speeches
        /// </summary>
        public void EvaluateSummariesOfAllSpeeches()
        {
            var speeches = _db.NLPSpeeches
                .Where(s => !string.IsNullOrEmpty(s.EnglishTranslationOfSpeech) && s.ProtocolNumber > 70)
                .ToList();

            foreach (var speech in speeches)
            {
                try
                {
                    _db.TextSummarizationEvaluationScores.Add(EvaluateSummaryOfSpeech(speech.ExtractiveSummary,
                    speech.Text, speech.Id, TextSummarizationMethods.TextRank));
                    _db.TextSummarizationEvaluationScores.Add(EvaluateSummaryOfSpeech(speech.AbstractSummaryPEGASUS,
                        speech.Text, speech.Id, TextSummarizationMethods.PEGASUSSamSum));
                    _db.TextSummarizationEvaluationScores.Add(EvaluateSummaryOfSpeech(speech.AbstractSummary,
                        speech.Text, speech.Id, TextSummarizationMethods.BARTSamSum));

                    _db.SaveChanges();
                }
                catch( Exception ex)
                {
                    _logger.LogError(ex, "Couldn't text summarize evalute the speech: " + speech.Id);
                }
            }
        }

        /// <summary>
        /// Evaluates the summary of a given speech and returns the score.
        /// </summary>
        public TextSummarizationEvaluationScore EvaluateSummaryOfSpeech(string summary, 
            string fullText, 
            Guid speechId,
            TextSummarizationMethods method)
        {
            var evaluation = new TextSummarizationEvaluationScore()
            {
                SpeechId = speechId,
                TextSummarizationMethod = method,
                ScoreExplanation = "Der Wert berechnet sich aus folgenden Abzügen:\n\n",
                Created = DateTime.Now
            };

            if (string.IsNullOrEmpty(summary))
            {
                evaluation.SummaryScore = 0;
                evaluation.ScoreExplanation += "Zusammenfassung war leer.";
                return evaluation;
            };

            // Check the length of the summary.
            var summaryCompressionRate = (100.0 / fullText.Length) * summary.Length;

            // Get the levenstein distance from each sentence
            var levenshtein = new Levenshtein();
            var sentences = Regex.Split(summary, @"(?<=[\.!\?])\s+");
            var levensteinValues = new List<double>();
            for (int i = 0; i < sentences.Length; i++)
            {
                // Get the levenstein distance between all sentence, but no duplicates
                var curSentence = sentences[i];
                for (int j = i + 1; j < sentences.Length; j++)
                {
                    var compare = sentences[j];
                    // Dies ist ein Satz
                    // Dies ist ein zweiter Satz
                    // LV: 8. Its 8 characters apart.
                    double lv = levenshtein.Compute(curSentence, compare);
                    // Calculate the similarity of two sentences on a decimal value like 0.34
                    levensteinValues.Add(1 - lv/Math.Max(curSentence.Length, compare.Length));
                }
            }

            // Also calculate the average sentence length
            var avgWordsPerSentence = sentences.Sum(s => s.Split(" ").Length) / sentences.Length;

            // Check the NE density and ratio. We gather all nes which occur more than once in the orignal text
            // We then check if these ne occur on the same ratio in the summary or in a similar ratio.
            // We want the same NE ratio in the text as in the summary!
            var topNes = _db.NamedEntity.Where(ne => ne.NLPSpeechId == speechId && ne.ShoutId == Guid.Empty)
                .AsEnumerable()
                .GroupBy(ne => ne.LemmaValue)
                .Select(g => Tuple.Create(g.First().LemmaValue, g.Count()))
                .OrderByDescending(t => t.Item2)
                .ToList();
            var softmax = Accord.Math.Special
                .Softmax(topNes.Select(t => (double)t.Item2)
                .ToArray());
            var topNesTextSoftmaxed = new List<(string, double)>();
            for (int i = 0; i < topNes.Count; i++)
            {
                // Example: ("FDP", 0.6858)
                topNesTextSoftmaxed.Add((topNes[i].Item1, softmax[i]));
            }

            // Now check the summary for the extracted NES
            var topNesSummary = new List<(string, double)>();
            foreach(var ne in topNesTextSoftmaxed)
            {
                var value = ne.Item1;
                var occurencesInSummary = summary.ToLower().Split(value.ToLower()).Count() - 1;
                // This counts the occurences in the summary. We softmax that later. The problem:
                // Softmaxing on the full text has more potential of very steep values cause of more
                // occurence possibilties. Thats why we have to take in the length percentage of the 
                // summary as well so we can normalize that.
                topNesSummary.Add((value, occurencesInSummary));
            }
            var softmaxSummary = Accord.Math.Special.Softmax(topNesSummary.Select(t => (double)t.Item2).ToArray());
            var topNesSummarySoftmaxed = new List<(string, double)>();

            for (int i = 0; i < topNesSummary.Count; i++)
            {
                // Example: ("FDP", 0.6858)
                topNesSummarySoftmaxed.Add((topNesSummary[i].Item1, softmaxSummary[i]));
            }

            // =============================================================================
            var finalScoreValue = 10;
            // Now calculate the full score with the value we got at hand
            // Calculate the total distance of the ne vector. Typically a value of 0.3-1.5
            double neDistance = 0;
            for (int i = 0; i < topNesTextSoftmaxed.Count; i++)
            {
                neDistance += MathHelper.GetDistanceBetweenTwoPoints(
                    topNesSummarySoftmaxed[i].Item2, topNesTextSoftmaxed[i].Item2);
                evaluation.NamedEntityDistance = neDistance;
            }
            evaluation.NamedEntityDistance = neDistance;
            if (neDistance >= 0.75)
            {
                var value = 0;
                if (neDistance >= 3) value += 5;
                else if (neDistance >= 2) value += 4;
                else if (neDistance >= 1) value += 3;
                else if (neDistance >= 0.75) value += 2;
                finalScoreValue -= value;
                evaluation.ScoreExplanation += "Zusammenfassung hatte keine optimale Named-Entity-Relations-Dichte. " +
                    $"\n- Abzug von {value} Punkten.\n";
            }


            // Similarities of sentences with levensthein
            // If two sentences are more than 50% equal, then add one point
            var counter = 1;
            foreach (var similarity in levensteinValues)
            {
                if (similarity >= 0.4)
                {
                    var value = 0;
                    if (similarity == 1) value += 5;
                    else if (similarity >= 0.75) value += 3;
                    else if (similarity >= 0.4) value += 2;
                    finalScoreValue -= value;
                    evaluation.ScoreExplanation += $"Die Sätze hatten zu große Ähnlichkeiten miteinander." +
                        $"\n- Abzug von {value} Punkten.\n";
                }
                counter++;
            }
            evaluation.LevenstheinSimilaritiesOfSentences = String.Join(";", levensteinValues);


            // Averga sentence length. Every 30 words per average one point
            if(avgWordsPerSentence >= 20)
            {
                var value = avgWordsPerSentence / 10;
                finalScoreValue -= value;
                evaluation.ScoreExplanation += $"Die Sätze hatten im Durchschnitt zu viele Worte." +
                    $"\n- Abzug von {value} Punkten.\n";
            }
            evaluation.AverageWordsPerSentence = avgWordsPerSentence;

            // SummaryComparedToText Length.
            if (summaryCompressionRate >= 30)
            {
                var value = (int)summaryCompressionRate / 15;
                finalScoreValue -= value;
                evaluation.ScoreExplanation += $"Die Zusammenfassung war insgesamt zu lang." +
                    $"\n- Abzug von {value} Punkten.\n";
            }
            else if (summaryCompressionRate <= 10)
            {
                var value = 2;
                finalScoreValue -= value;
                evaluation.ScoreExplanation += $"Die Länge der Zusammenfassung war nichtmal 10% der originalen Rede. " +
                    $"\n- Abzug von {value} Punkten.\n";
            }
            evaluation.SummaryCompressionRate = summaryCompressionRate;

            // 10 is the max final score. It doesnt get any worse.
            if(finalScoreValue < 0) finalScoreValue = 0;

            evaluation.SummaryScore = finalScoreValue;
            return evaluation;
        }
    }
}
