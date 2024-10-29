using BundestagMine.Models.Database;
using BundestagMine.Models.Database.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.Utility;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        /// <summary>
        /// Calls a python script, waits until its finished and read in the output. Return that output
        /// </summary>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        public dynamic? ExecutePythonScript(string workingDirectory,
            string scriptName,
            string scriptInput,
            out string status)
        {
            status = "";
            // We communicate with the python scripts via input and output files
            var inputFileName = Path.Combine(workingDirectory, $"{scriptName.Replace(".py", "")}_input.txt");
            var outputFileName = Path.Combine(workingDirectory, $"{scriptName.Replace(".py", "")}_output.json");

            try
            {
                File.WriteAllText(inputFileName, scriptInput);
                var startInfo = new ProcessStartInfo(ConfigManager.GetPythonExePath())
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    Arguments = $"\"{scriptName}\"",
                    WorkingDirectory = workingDirectory,
                    StandardOutputEncoding = Encoding.UTF8,
                };

                // Start the script
                using (var process = Process.Start(startInfo))
                {
                    using (var reader = process?.StandardOutput)
                    {
                        // Wait until its finished
                        status = reader?.ReadToEnd();
                        var result = File.ReadAllText(outputFileName);
                        dynamic asJson = JsonConvert.DeserializeObject(result);
                        return asJson;
                    }
                }
            }
            catch (Exception ex)
            {
                status = "BAD";
                _logger.LogError(ex, "Unknown error when trying to execute python script: " + scriptName);
                return null;
            }
            finally
            {
                // Delete the text files at the end
                if (File.Exists(inputFileName))
                    File.Delete(inputFileName);

                if (File.Exists(outputFileName))
                    File.Delete(outputFileName);
            }
        }

        /// <summary>
        /// The summarization pipeline consists of the following steps:
        /// 1. Translation of the german speech to english
        /// 2. Summarizing the english text with BART and PEGASUS, summarize the german text with TextRank
        /// 3. Translating the english summary back to german
        /// 4. Evaluating the english translation with a score from 0-1 of how good the translation was
        /// 5. Evaluating the german summary of how good the summary is
        /// </summary>
        /// <returns></returns>
        public (NLPSpeech, List<TextSummarizationEvaluationScore>) RunSpeechThroughSummarizationPipeline(NLPSpeech speech)
        {
            // The pipeline calls to python scripts, which use the transformer to calculate the operations
            // we need.

            // We really dont need to summarize small speeches...
            if (speech.Text.Length < 400) return (speech, new List<TextSummarizationEvaluationScore>());

            // Step 1: Translate the speech to english
            if (string.IsNullOrEmpty(speech.EnglishTranslationOfSpeech))
            {
                var result = ExecutePythonScript(
                    ConfigManager.GetPythonScriptPath(), ConfigManager.GetPythonScriptName_Translation(),
                    speech.Text, out var status);
                if (result != null && status.Contains("GOOD"))
                    speech.EnglishTranslationOfSpeech = result?.english_translation;
            }

            // Step 2: Evalute the translation
            if(speech.EnglishTranslationScore == 0.0 || speech.EnglishTranslationScore == 0)
            {
                var result = ExecutePythonScript(
                    ConfigManager.GetPythonScriptPath(), ConfigManager.GetPythonScriptName_Translation_Evaluation(),
                    JsonConvert.SerializeObject(new { german = speech.Text, english = speech.EnglishTranslationOfSpeech}), out var status);
                if (result != null && status.Contains("GOOD"))
                    speech.EnglishTranslationScore = result?.similarity;
            }

            // Step 3: Summarize
            // TextRank
            if (string.IsNullOrEmpty(speech.ExtractiveSummary))
            {
                var result = ExecutePythonScript(
                    ConfigManager.GetPythonScriptPath(), ConfigManager.GetPythonScriptName_TextRank_Summary(),
                    speech.Text, out var status);
                if (result != null && status.Contains("GOOD"))
                    speech.ExtractiveSummary = result?.extractive_summary;
            }
            // PEGASUS
            if (string.IsNullOrEmpty(speech.AbstractSummaryPEGASUS))
            {
                var result = ExecutePythonScript(
                    ConfigManager.GetPythonScriptPath(), ConfigManager.GetPythonScriptName_PEGASUS_Summary(),
                    speech.EnglishTranslationOfSpeech, out var status);
                if (result != null && status.Contains("GOOD"))
                    speech.AbstractSummaryPEGASUS = result?.abstract_summary;
            }
            // BART
            if (string.IsNullOrEmpty(speech.AbstractSummary))
            {
                var result = ExecutePythonScript(
                    ConfigManager.GetPythonScriptPath(), ConfigManager.GetPythonScriptName_BART_Summary(),
                    speech.EnglishTranslationOfSpeech, out var status);
                if (result != null && status.Contains("GOOD"))
                    speech.AbstractSummary = result?.abstract_summary;
            }

            // Last step: Evalute the summaries
            var evaluations = new List<TextSummarizationEvaluationScore>();
            evaluations.Add(EvaluateSummaryOfSpeech(speech.ExtractiveSummary, speech.Text, speech.Id, TextSummarizationMethods.TextRank));
            evaluations.Add(EvaluateSummaryOfSpeech(speech.AbstractSummaryPEGASUS, speech.Text, speech.Id, TextSummarizationMethods.PEGASUSSamSum));
            evaluations.Add(EvaluateSummaryOfSpeech(speech.AbstractSummary, speech.Text, speech.Id, TextSummarizationMethods.BARTSamSum));

            // Return the results
            return (speech, evaluations);
        }

        public List<TextSummarizationEvaluationScore> GetEvaluationsOfSpeech(Guid speechId) =>
            _db.TextSummarizationEvaluationScores.Where(s => s.SpeechId == speechId).ToList();

        /// <summary>
        /// Starts the process of evaluating all automatic text summaries of all speeches
        /// </summary>
        public void EvaluateSummariesOfAllSpeeches()
        {
            var speeches = _db.NLPSpeeches
                .Where(s => !string.IsNullOrEmpty(s.EnglishTranslationOfSpeech)
                && (s.ProtocolNumber > 58 || s.ProtocolNumber < 13))
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
                catch (Exception ex)
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
            // If we already have an evaluation, dont do it again
            var existingEvaluation = _db.TextSummarizationEvaluationScores.FirstOrDefault(t => t.SpeechId == speechId
               && t.TextSummarizationMethod == method);
            if (existingEvaluation != default) return existingEvaluation;

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
                    levensteinValues.Add(1 - lv / Math.Max(curSentence.Length, compare.Length));
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
            foreach (var ne in topNesTextSoftmaxed)
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
                if (neDistance >= 3) value += 7;
                else if (neDistance >= 2) value += 5;
                else if (neDistance >= 1) value += 4;
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
            if (avgWordsPerSentence >= 20)
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
            if (finalScoreValue < 0) finalScoreValue = 0;

            evaluation.SummaryScore = finalScoreValue;
            return evaluation;
        }
    }
}
