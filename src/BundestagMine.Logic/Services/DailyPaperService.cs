using BundestagMine.Models.Database.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.Logic.ViewModels;
using BundestagMine.Logic.ViewModels.DailyPaper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using BundestagMine.Utility;
using System.Diagnostics;
using System.Collections;

namespace BundestagMine.Logic.Services
{
    public class DailyPaperService
    {
        private readonly PixabayApiService _pixabayApiService;
        private readonly AnnotationService _annotationService;
        private readonly MetadataService _metadataService;
        private readonly ILogger<DailyPaperService> _logger;
        private readonly BundestagMineDbContext _db;

        public DailyPaperService(BundestagMineDbContext db,
            ILogger<DailyPaperService> logger,
            MetadataService metadataService,
            AnnotationService annotationService,
            PixabayApiService pixabayApiService)
        {
            _pixabayApiService = pixabayApiService;
            _annotationService = annotationService;
            _metadataService = metadataService;
            _logger = logger;
            _db = db;
        }

        /// <summary>
        /// Gets the special themes of the day ordered and as tuples. Item1:NE, Item2:Count
        /// </summary>
        /// <param name="number"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        private List<(NamedEntity, int)> GetSpecialTopicsOfTheDay(int number, int period)
        {
            // When we leave all the parties in, the topic of the day is basically always SPD or any other
            // party. We dont want that as the special topic of the day, so we filter those out for now.
            var blacklist = _metadataService.GetFractionsAsStrings().Concat(_metadataService.GetPartiesAsStrings()).ToList();
            blacklist.Add("GRÜNEN");
            blacklist.Add("Deutschland");
            return _db.Speeches
                    .Where(s => s.ProtocolNumber == number
                        && s.LegislaturePeriod == period)
                    .AsEnumerable()
                    .SelectMany(s => _db.NamedEntity
                        .Where(ne => ne.NLPSpeechId == s.Id
                            && !TopicHelper.TopicBlackList.Contains(ne.LemmaValue.ToLower()) && ne.Value != "MISC"
                            && ne.ShoutId == Guid.Empty && !blacklist.Contains(ne.LemmaValue)))
                    .GroupBy(ne => ne.LemmaValue)
                    .OrderByDescending(ne => ne.Count())
                    .Take(15)
                    .Select(group =>
                    (
                        group.FirstOrDefault(),
                        group.Count()
                    ))
                    .Where(t => t.Item1 != default)
                    .ToList();
        }

        /// <summary>
        /// Builds a speechPartViewModel from a speech
        /// </summary>
        /// <param name="number"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        private SpeechPartViewModel BuildSpeechPartViewModel(Speech speech)
        {
            // Now build the viewmodel from it. 
            return new SpeechPartViewModel()
            {
                Speech = speech,
                Speaker = _db.Deputies.FirstOrDefault(d => d.SpeakerId == speech.SpeakerId),
                AgendaItem = _metadataService.GetAgendaItemOfSpeech(speech),
                ActualCommentsAmount = speech.Segments.Sum(ss => ss.Shouts.Where(sh => !sh.Text.ToLower().Contains("beifall")).Count()),
                Sentiment = _annotationService.GetAverageSentimentOfSpeech(speech),
                MostTwoUsedNamedEntity = _annotationService.GetMostUsedNamedEntitiesOfSpeech(speech, 2)
            };
        }

        /// <summary>
        /// Takes in a meetingId and legislaturePeriod and builds the viewmodel data for the paper.
        /// </summary>
        /// <returns></returns>
        public DailyPaperViewModel BuildDailyPaperViewModelAsync(int meetingNumber, int legislaturePeriod)
        {
            try
            {
                var dailyPaperViewModel = new DailyPaperViewModel();

                // Get the protocol
                dailyPaperViewModel.Protocol = _db.Protocols
                    .FirstOrDefault(p => p.LegislaturePeriod == legislaturePeriod && p.Number == meetingNumber);

                if (dailyPaperViewModel.Protocol == default) return null;

                // Get the agenda items
                dailyPaperViewModel.AgendaItems = _db.AgendaItems
                    .Where(ag => ag.ProtocolId == dailyPaperViewModel.Protocol.Id)
                    .AsEnumerable()
                    .Select(a =>
                    (
                        a,
                        _metadataService.GetSpeechesCountOfAgendaItem(legislaturePeriod, meetingNumber, a.Order)
                    ))
                    .OrderBy(t => t.Item1.Order)
                    .ToList();

                // Determine the theme of the day. That is the ne with the most occurences in all speeches.
                dailyPaperViewModel.NamedEntitiesOfTheDay = GetSpecialTopicsOfTheDay(meetingNumber, legislaturePeriod);

                // Get the sentiment of each named entity of the day for the stacked bar chart. We sort this in js.
                var data = new List<dynamic>();
                foreach (var ne in dailyPaperViewModel.NamedEntitiesOfTheDay)
                {
                    // object like: 
                    //  { Count = e.Count(), // This is the amount of occcurences
                    //  Value = e.Key } // This is neg, neu or pos
                    var sentimentOfNe = _annotationService.GetNamedEntityWithCorrespondingSentiment(
                        ne.Item1.LemmaValue, dailyPaperViewModel.Protocol.Date, dailyPaperViewModel.Protocol.Date, "", "", "");
                    data.Add(new
                    {
                        ne = ne.Item1.LemmaValue,
                        neOccurences = ne.Item2,
                        sentiments = sentimentOfNe
                    });

                    // We take this oppurtinity and determine the most neg, neu and pos namedentity.
                    // We do that by calculating by percentage.
                    var iterable = ((IEnumerable)sentimentOfNe).Cast<dynamic>();
                    var neg = iterable.FirstOrDefault(i => i.Value == "neg");
                    var pos = iterable.FirstOrDefault(i => i.Value == "pos");
                    var neu = iterable.FirstOrDefault(i => i.Value == "neu");
                    double total = 0;
                    if (neg?.Count != null) total += neg.Count;
                    if (pos?.Count != null) total += pos.Count;
                    if (neu?.Count != null) total += neu.Count;

                    var negPercentage = 100 / (double)total * neg?.Count;
                    if (negPercentage > dailyPaperViewModel.MostNegativNamedEntity.Item2)
                        dailyPaperViewModel.MostNegativNamedEntity = (ne.Item1.LemmaValue, negPercentage);

                    var posPercentage = 100 / (double)total * pos?.Count;
                    if (posPercentage > dailyPaperViewModel.MostPositiveNamedEntity.Item2)
                        dailyPaperViewModel.MostPositiveNamedEntity = (ne.Item1.LemmaValue, posPercentage);

                    var neuPercentage = 100 / (double)total * neu?.Count;
                    if (neuPercentage > dailyPaperViewModel.MostNeutralNamedEntity.Item2)
                        dailyPaperViewModel.MostNeutralNamedEntity = (ne.Item1.LemmaValue, neuPercentage);
                }
                dailyPaperViewModel.NamedEntitiesOfTheDayChartData = data;

                // Testing
                //for (int i = 1; i < 75; i++)
                //{
                //    var th = GetSpecialTopicsOfTheDay(i, 20);
                //    if (th == null) continue;
                //}

                // Get the thumbnail
                dailyPaperViewModel.Thumbnail = _pixabayApiService
                    .SearchForImageAsync(dailyPaperViewModel.NamedEntitiesOfTheDay.First().Item1.LemmaValue, "")
                    .GetAwaiter().GetResult();

                // Gets the most controverse speech.
                var mostCommentedsSpeech = _db.Speeches
                    .Where(s => s.ProtocolNumber == meetingNumber && s.LegislaturePeriod == legislaturePeriod)
                    .AsEnumerable()
                    .OrderByDescending(s => _metadataService.GetActualCommentsAmountOfSpeech(s))
                    .First();
                dailyPaperViewModel.MostCommentedSpeech = BuildSpeechPartViewModel(mostCommentedsSpeech);
                dailyPaperViewModel.MostCommentedSpeech.SpeechPartType = SpeechPartType.MostCommented;

                // Gets the most positive speech.
                var mostPositiveSpeech = _db.Speeches
                    .Where(s => s.ProtocolNumber == meetingNumber && s.LegislaturePeriod == legislaturePeriod)
                    .AsEnumerable()
                    .OrderByDescending(s => _db.Sentiment.Where(se => se.NLPSpeechId == s.Id).AverageOrDefault(se => se.SentimentSingleScore))
                    .First();
                dailyPaperViewModel.MostPositiveSpeech = BuildSpeechPartViewModel(mostPositiveSpeech);
                dailyPaperViewModel.MostPositiveSpeech.SpeechPartType = SpeechPartType.MostPositive;

                // Gets the most negative speech.
                var mostNegativeSpeech = _db.Speeches
                    .Where(s => s.ProtocolNumber == meetingNumber && s.LegislaturePeriod == legislaturePeriod)
                    .AsEnumerable()
                    .OrderBy(s => _db.Sentiment.Where(se => se.NLPSpeechId == s.Id).AverageOrDefault(se => se.SentimentSingleScore))
                    .First();
                dailyPaperViewModel.MostNegativeSpeech = BuildSpeechPartViewModel(mostNegativeSpeech);
                dailyPaperViewModel.MostNegativeSpeech.SpeechPartType = SpeechPartType.MostNegative;

                // Get all speakers. We also need their speeches counts 
                var allSpeakers = _metadataService.GetAllSpeakersOfProtocol(meetingNumber, legislaturePeriod);
                dailyPaperViewModel.AllSpeakers = allSpeakers
                    .GroupBy(s => s)
                    .Select(s => (s.Key, s.Count()))
                    .OrderByDescending(t => t.Item2)
                    .ToList();

                // Calculate the average sentiment of all speeches of the protocol
                dailyPaperViewModel.AverageSentiment = _db.Speeches
                    .Where(s => s.LegislaturePeriod == legislaturePeriod && s.ProtocolNumber == meetingNumber)
                    .AsEnumerable()
                    .Average(s => _annotationService.GetAverageSentimentOfSpeech(s));

                // Calculate the total sentiment distribution
                dailyPaperViewModel.TotalSentimentChartDistribution = _annotationService.GetSentimentsForGraphs(
                    dailyPaperViewModel.Protocol.Date, dailyPaperViewModel.Protocol.Date, "", "", "");

                // Calculate the sentiment foreach fraction
                var fractionsOfProtocol = _metadataService.GetFractionsOfProtocol(legislaturePeriod, meetingNumber);
                dailyPaperViewModel.FractionSentimentCharts = new List<(string, dynamic)>();
                foreach(var fraction in fractionsOfProtocol)
                {
                    var sentimentData = _annotationService.GetSentimentsForGraphs(
                    dailyPaperViewModel.Protocol.Date, dailyPaperViewModel.Protocol.Date, fraction, "", "");
                    // Spaces, slashes and umlaute destroy the js in the frontend.
                    dailyPaperViewModel.FractionSentimentCharts
                        .Add((fraction, sentimentData));
                }

                return dailyPaperViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Couldn't build the daily paper view model, error:");
                return null;
            }
        }
    }
}
