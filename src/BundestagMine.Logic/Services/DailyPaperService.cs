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
using BundestagMine.Logic.HelperModels.DailyPaper;
using BundestagMine.Models.Database;
using Newtonsoft.Json;

namespace BundestagMine.Logic.Services
{
    public class DailyPaperService
    {
        private readonly GraphDataService _graphDataService;
        private readonly PixabayApiService _pixabayApiService;
        private readonly AnnotationService _annotationService;
        private readonly MetadataService _metadataService;
        private readonly ILogger<DailyPaperService> _logger;
        private readonly BundestagMineDbContext _db;

        public DailyPaperService(BundestagMineDbContext db,
            ILogger<DailyPaperService> logger,
            MetadataService metadataService,
            AnnotationService annotationService,
            PixabayApiService pixabayApiService,
            GraphDataService graphDataService)
        {
            _graphDataService = graphDataService;
            _pixabayApiService = pixabayApiService;
            _annotationService = annotationService;
            _metadataService = metadataService;
            _logger = logger;
            _db = db;
        }

        /// <summary>
        /// Gets the newest daily paper
        /// </summary>
        /// <returns></returns>
        public DailyPaper GetNewestDailyPaper() => _db.DailyPapers
            .OrderByDescending(d => d.LegislaturePeriod)
            .ThenByDescending(d => d.ProtocolNumber)
            .First();

        /// <summary>
        /// Gets all subscriptions which arent up to date in receving their daily paper.
        /// </summary>
        /// <returns></returns>
        public List<DailyPaperSubscription> GetNotUpToDateSubscriptions() => _db.DailyPaperSubscriptions
            .Where(s => s.Active && s.LastSentDailyPaperId != GetNewestDailyPaper().Id)
            .ToList();

        /// <summary>
        /// Gets a dailypaperviewmodel from the db
        /// </summary>
        /// <param name="period"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public DailyPaperViewModel GetDailyPaperAsViewModel(int period, int number)
        {
            var dp = _db.DailyPapers.FirstOrDefault(d => d.LegislaturePeriod == period && d.ProtocolNumber == number);
            if (dp == null) return null;
            return JsonConvert.DeserializeObject<DailyPaperViewModel>(dp.JsonDataString);
        }

        /// <summary>
        /// Gets all protocols without a daily paper to it
        /// </summary>
        /// <returns></returns>
        public List<Protocol> GetProtocolsWithoutDailyPaper() => _db.Protocols
            .Where(p => !_db.DailyPapers.Any(d => (d.LegislaturePeriod == p.LegislaturePeriod && d.ProtocolNumber == p.Number)))
            .ToList();

        /// <summary>
        /// Gets all protocols with a daily paper to it
        /// </summary>
        /// <returns></returns>
        public List<Protocol> GetProtocolsWithDailyPaper() => _db.Protocols
            .Where(p => _db.DailyPapers
              .Any(d => d.LegislaturePeriod == p.LegislaturePeriod && d.ProtocolNumber == p.Number))
            .ToList();

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
        /// Gets the agendaitems with their speech counts as item2
        /// </summary>
        /// <param name="protocolId"></param>
        /// <param name="period"></param>
        /// <param name="meetingNumber"></param>
        /// <returns></returns>
        private List<(AgendaItem, int)> BuildAgendaItems(Guid protocolId, int period, int meetingNumber) =>
            _db.AgendaItems
                    .Where(ag => ag.ProtocolId == protocolId)
                    .AsEnumerable()
                    .Select(a =>
                    (
                        a,
                        _metadataService.GetSpeechesCountOfAgendaItem(period, meetingNumber, a.Order)
                    ))
                    .OrderBy(t => t.Item1.Order)
                    .ToList();

        /// <summary>
        /// Builds the data for the stacked ne chart
        /// </summary>
        /// <returns></returns>
        private DailyPaperViewModel BuildNamedEntitiesOfTheDayChartData(DailyPaperViewModel dailyPaperViewModel)
        {
            var data = new List<NamedEntityChartData>();
            foreach (var ne in dailyPaperViewModel.NamedEntitiesOfTheDay)
            {
                // object like: 
                //  { Count = e.Count(), // This is the amount of occcurences
                //  Value = e.Key } // This is neg, neu or pos
                // This is shit with all the dynamics, but its old code...
                var sentimentOfNe = _annotationService.GetNamedEntityWithCorrespondingSentiment(
                    ne.Item1.LemmaValue, dailyPaperViewModel.Protocol.Date, dailyPaperViewModel.Protocol.Date, "", "", "");
                var iterable = ((IEnumerable)sentimentOfNe).Cast<dynamic>();
                var sentimentData = iterable.Select(i => new SentimentChartData()
                {
                    Count = i.Count,
                    Value = i.Value,
                }).ToList();

                data.Add(new NamedEntityChartData()
                {
                    NamedEntity = ne.Item1.LemmaValue,
                    NamedEntityOccurences = ne.Item2,
                    Sentiments = sentimentData
                });

                // We take this oppurtinity and determine the most neg, neu and pos namedentity.
                // We do that by calculating by percentage.
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

            return dailyPaperViewModel;
        }

        /// <summary>
        /// Builds the polls
        /// </summary>
        /// <param name="period"></param>
        /// <param name="meetingNumber"></param>
        /// <returns></returns>
        private List<(Poll, List<PollChartData>)> BuildPolls(int period, int meetingNumber)
        {
            var polls = _db.Polls
                .Where(p => p.LegislaturePeriod == period && p.ProtocolNumber == meetingNumber)
                .Include(p => p.Entries)
                .ToList();
            var pollData = new List<(Poll, List<PollChartData>)>();
            // Iterate through them and prepare them for the charts
            foreach (var poll in polls)
            {
                pollData.Add((
                    poll,
                    poll.Entries
                    .GroupBy(e => e.VoteResultAsGermanString)
                    .Select(g => new PollChartData()
                    {
                        Count = g.Count(),
                        PollResult = g.FirstOrDefault() == null ? "" : g.First().VoteResultAsGermanString
                    })
                    .Where(p => !string.IsNullOrEmpty(p.PollResult))
                    .ToList()
                ));
            }

            return pollData;
        }

        /// <summary>
        /// Builds the facts and numbers of the paper
        /// </summary>
        /// <returns></returns>
        private DailyPaperViewModel BuildFacts(DailyPaperViewModel dailyPaperViewModel)
        {
            // Total speeches
            dailyPaperViewModel.TotalSpeechesCount = _db.Speeches
                .Where(s => s.ProtocolNumber == dailyPaperViewModel.Protocol.Number &&
                    s.LegislaturePeriod == dailyPaperViewModel.Protocol.LegislaturePeriod)
                .Count();

            // longest speech
            var longestSpeech = _db.Speeches
                .Where(s => s.ProtocolNumber == dailyPaperViewModel.Protocol.Number &&
                    s.LegislaturePeriod == dailyPaperViewModel.Protocol.LegislaturePeriod)
                .OrderByDescending(s => s.Text.Length)
                .First();
            dailyPaperViewModel.LongestSpeech = new SpeechViewModel()
            {
                Speech = longestSpeech,
                Speaker = _db.Deputies.FirstOrDefault(d => d.SpeakerId == longestSpeech.SpeakerId),
                Agenda = _metadataService.GetAgendaItemOfSpeech(longestSpeech)
            };

            // most commented deputy
            // Lets do this without LINQ... speed is not important here and it kinda sucks here
            var speakerIdToShouts = new Dictionary<string, int>();
            foreach (var speech in _db.Speeches
                .Where(s => s.ProtocolNumber == dailyPaperViewModel.Protocol.Number &&
                    s.LegislaturePeriod == dailyPaperViewModel.Protocol.LegislaturePeriod).ToList())
            {
                var shouts = _metadataService.GetActualCommentsOfSpeech(speech);
                foreach(var shout in shouts)
                {
                    if (string.IsNullOrEmpty(shout.SpeakerId)) continue;
                    // Store the speaker and add up their comments
                    if (speakerIdToShouts.ContainsKey(shout.SpeakerId))
                        speakerIdToShouts[shout.SpeakerId] = speakerIdToShouts[shout.SpeakerId] + 1;
                    else speakerIdToShouts[shout.SpeakerId] = 0;
                }
            }
            dailyPaperViewModel.MostCommentsByDeputy = speakerIdToShouts.OrderByDescending(kv => kv.Value)
                .Select(kv => (_db.Deputies.FirstOrDefault(d => d.SpeakerId == kv.Key), kv.Value))
                .FirstOrDefault();

            // most applauded
            // Lets do this without LINQ... speed is not important here and it kinda sucks here
            var mostApplaudedSpeech = (new Speech(), 0);
            foreach(var speech in _db.Speeches
                .Where(s => s.ProtocolNumber == dailyPaperViewModel.Protocol.Number &&
                    s.LegislaturePeriod == dailyPaperViewModel.Protocol.LegislaturePeriod).ToList())
            {
                var shouts = _metadataService.GetApplaudCommentsOfSpeech(speech);
                if (shouts.Count > mostApplaudedSpeech.Item2)
                    mostApplaudedSpeech = (speech, shouts.Count);
            }
            dailyPaperViewModel.MostApplaudedSpeech = new SpeechViewModel()
            {
                Speech = mostApplaudedSpeech.Item1,
                Speaker = _db.Deputies.FirstOrDefault(d => d.SpeakerId == mostApplaudedSpeech.Item1.SpeakerId),
                Agenda = _metadataService.GetAgendaItemOfSpeech(mostApplaudedSpeech.Item1),
                ApplaudeCount = mostApplaudedSpeech.Item2
            };

            return dailyPaperViewModel;
        }

        /// <summary>
        /// Builds the dailypaper for a given protocol
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public DailyPaperViewModel BuildDailyPaperViewModel(Protocol protocol) =>
            BuildDailyPaperViewModel(protocol.Number, protocol.LegislaturePeriod);

        /// <summary>
        /// Takes in a meetingId and legislaturePeriod and builds the viewmodel data for the paper.
        /// </summary>
        /// <returns></returns>
        public DailyPaperViewModel BuildDailyPaperViewModel(int meetingNumber, int legislaturePeriod)
        {
            try
            {
                var dailyPaperViewModel = new DailyPaperViewModel();

                // Get the protocol
                dailyPaperViewModel.Protocol = _db.Protocols
                    .FirstOrDefault(p => p.LegislaturePeriod == legislaturePeriod && p.Number == meetingNumber);

                if (dailyPaperViewModel.Protocol == default) return null;

                // Get the agenda items
                dailyPaperViewModel.AgendaItems = BuildAgendaItems(dailyPaperViewModel.Protocol.Id, legislaturePeriod, meetingNumber);

                // Determine the theme of the day. That is the ne with the most occurences in all speeches.
                dailyPaperViewModel.NamedEntitiesOfTheDay = GetSpecialTopicsOfTheDay(meetingNumber, legislaturePeriod);

                // Get the sentiment of each named entity of the day for the stacked bar chart. We sort this in js.
                dailyPaperViewModel = BuildNamedEntitiesOfTheDayChartData(dailyPaperViewModel);

                // Get the thumbnail
                dailyPaperViewModel.Thumbnail = _pixabayApiService
                    .SearchForImageAsync(dailyPaperViewModel.NamedEntitiesOfTheDay.First().Item1.LemmaValue, "")
                    .GetAwaiter().GetResult();
                // If the main topic doesnt have any images, search for the second topic
                if (dailyPaperViewModel.Thumbnail == null && dailyPaperViewModel.NamedEntitiesOfTheDay.Count > 1)
                    dailyPaperViewModel.Thumbnail = _pixabayApiService
                        .SearchForImageAsync(dailyPaperViewModel.NamedEntitiesOfTheDay[1].Item1.LemmaValue, "")
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
                dailyPaperViewModel.FractionSentimentCharts = new List<(string, List<SentimentChartData>)>();
                foreach (var fraction in fractionsOfProtocol)
                {
                    var sentimentData = _annotationService.GetSentimentsForGraphs(
                    dailyPaperViewModel.Protocol.Date, dailyPaperViewModel.Protocol.Date, fraction, "", "");
                    dailyPaperViewModel.FractionSentimentCharts.Add((fraction, sentimentData));
                }

                // Calculate the polls. For the polldata we get objects with the result and their result counts
                dailyPaperViewModel.Polls = BuildPolls(legislaturePeriod, meetingNumber);

                // Builds the facts
                dailyPaperViewModel = BuildFacts(dailyPaperViewModel);

                // Build the comment network
                dailyPaperViewModel.CommentNetworkData = _graphDataService.GetActualCommentNetworkOfProtocol(legislaturePeriod, meetingNumber);

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
