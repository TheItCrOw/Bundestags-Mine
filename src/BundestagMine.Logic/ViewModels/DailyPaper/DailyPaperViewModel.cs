using BundestagMine.Logic.HelperModels.DailyPaper;
using BundestagMine.Logic.RequestModels.Pixabay;
using BundestagMine.Models.Database;
using BundestagMine.Models.Database.MongoDB;
using System.Collections.Generic;

namespace BundestagMine.Logic.ViewModels.DailyPaper
{
    /// <summary>
    /// The viewmodel that holds all information to visualise a daily paper. This model gets json serialized 
    /// so be aware of that when using dynamic objects or the likes.
    /// </summary>
    public class DailyPaperViewModel
    {
        public Protocol Protocol { get; set; }
        public SearchHit Thumbnail { get; set; }


        #region ne
        /// <summary>
        /// The ne with the most occurencens in all speeches.
        /// </summary>
        public List<(NamedEntity, int)> NamedEntitiesOfTheDay { get; set; }
        public List<NamedEntityChartData> NamedEntitiesOfTheDayChartData { get; set; }
        public string FirstSpecialTopicOfTheDay { get => NamedEntitiesOfTheDay.Count > 0 ? NamedEntitiesOfTheDay.FirstOrDefault().Item1.LemmaValue : ""; }
        public string SecondSpecialTopicOfTheDay { get => NamedEntitiesOfTheDay.Count > 1 ? NamedEntitiesOfTheDay[1].Item1.LemmaValue : ""; }
        public string ThirdSpecialTopicOfTheDay { get => NamedEntitiesOfTheDay.Count > 2 ? NamedEntitiesOfTheDay[2].Item1.LemmaValue : ""; }
        public string SecondLastSpecialTopicOfTheDay { get => NamedEntitiesOfTheDay.Count > 4 ? NamedEntitiesOfTheDay[NamedEntitiesOfTheDay.Count - 2].Item1.LemmaValue : ""; }
        public string LastSpecialTopicOfTheDay { get => NamedEntitiesOfTheDay.Count > 3 ? NamedEntitiesOfTheDay[NamedEntitiesOfTheDay.Count - 1].Item1.LemmaValue : ""; }

        /// <summary>
        /// Item1: Ne, Item2: Percentage
        /// </summary>
        public (string, double) MostNegativNamedEntity { get; set; }
        /// <summary>
        /// Item1: Ne, Item2: Percentage
        /// </summary>
        public (string, double) MostNeutralNamedEntity { get; set; }
        /// <summary>
        /// Item1: Ne, Item2: Percentage
        /// </summary>
        public (string, double) MostPositiveNamedEntity { get; set; }
        #endregion

        #region agenda
        // ======================================= Agenda Items
        /// <summary>
        /// Item1: Agendaitem, Item2: AmountOfSpeech in it
        /// </summary>
        public List<(AgendaItem, int)> AgendaItems { get; set; }
        private List<(AgendaItem, int)> GetAgendaItemsSortedBySpeechesCount
        {
            get => AgendaItems.OrderByDescending(a => a.Item2).ToList();
        }
        public (string, int) MostSpeechesAgendaItem
        {
            get => AgendaItems.Count > 0
                ? (GetAgendaItemsSortedBySpeechesCount.First().Item1.Title, GetAgendaItemsSortedBySpeechesCount.First().Item2)
                : ("", 0);
        }
        public (string, int) SecondMostSpeechesAgendaItem
        {
            get => AgendaItems.Count > 1
                ? (GetAgendaItemsSortedBySpeechesCount[1].Item1.Title, GetAgendaItemsSortedBySpeechesCount[1].Item2)
                : ("", 0);
        }
        public (string, int) ThirdMostSpeechesAgendaItem
        {
            get => AgendaItems.Count > 2
                ? (GetAgendaItemsSortedBySpeechesCount[2].Item1.Title, GetAgendaItemsSortedBySpeechesCount[2].Item2)
                : ("", 0);
        }
        public (string, int) LastMostSpeechesAgendaItem
        {
            get => AgendaItems.Count > 0
                ? (GetAgendaItemsSortedBySpeechesCount.Where(a => a.Item2 > 0).Last().Item1.Title, GetAgendaItemsSortedBySpeechesCount.Where(a => a.Item2 > 0).ToList().Last().Item2)
                : ("", 0);
        }
        public dynamic GetChartFormattedAgendaItems
        {
            get => GetAgendaItemsSortedBySpeechesCount.Select(t => new { value = t.Item1.Title, count = t.Item2 });
        }
        #endregion

        #region sentiments
        public double AverageSentiment { get; set; }
        /// <summary>
        /// The mood of the meeting
        /// </summary>
        public string MeetingMood
        {
            get
            {
                if (AverageSentiment < -0.5) return "erhitzt";
                else if (AverageSentiment < -0.2) return "negativ";
                else if (AverageSentiment < -0.05) return "eher negativ";
                else if (AverageSentiment < 0.05 && AverageSentiment > -0.05) return "zurückhaltend";
                else if (AverageSentiment > 0.5) return "hoffnungsvoll";
                else if (AverageSentiment > 0.2) return "positiv";
                else if (AverageSentiment > 0.05) return "eher positiv";
                return "Unbekannt";
            }
        }
        /// <summary>
        /// Chart data for total sentiment
        /// </summary>
        public List<SentimentChartData> TotalSentimentChartDistribution { get; set; }
        /// <summary>
        /// This holds the data for sentiment charts foreach fraction holding a speech
        /// </summary>
        public List<(string, List<SentimentChartData>)> FractionSentimentCharts { get; set; }

        #endregion

        /// <summary>
        /// The polls of this meeting
        /// </summary>
        public List<(Poll, List<PollChartData>)> Polls { get; set; }

        #region speeches
        public SpeechPartViewModel MostCommentedSpeech { get; set; }
        public SpeechPartViewModel MostPositiveSpeech { get; set; }
        public SpeechPartViewModel MostNegativeSpeech { get; set; }
        #endregion

        #region facts

        public int TotalSpeechesCount { get; set; }
        public SpeechViewModel LongestSpeech { get; set; }
        public (Deputy?, int) MostCommentsByDeputy { get; set; }

        public SpeechViewModel MostApplaudedSpeech { get; set; }

        public (Deputy?, int) MostSpeechesByDeputy { get => AllSpeakers != null ? AllSpeakers.FirstOrDefault() : (null, 0); }

        #endregion

        /// <summary>
        /// The data for the comment network chart
        /// </summary>
        public NetworkData CommentNetworkData { get; set; }

        /// <summary>
        /// Ordered list of all speakers with their speeches amount
        /// </summary>
        public List<(Deputy?, int)> AllSpeakers { get; set; }
    }
}
