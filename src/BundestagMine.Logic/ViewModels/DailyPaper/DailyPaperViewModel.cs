using BundestagMine.Logic.RequestModels.Pixabay;
using BundestagMine.Models.Database.MongoDB;
using System.Collections.Generic;

namespace BundestagMine.Logic.ViewModels.DailyPaper
{
    public class DailyPaperViewModel
    {
        public Protocol Protocol { get; set; }
        public SearchHit Thumbnail { get; set; }


        // ======================================= NE
        /// <summary>
        /// The ne with the most occurencens in all speeches.
        /// </summary>
        public List<(NamedEntity, int)> NamedEntitiesOfTheDay { get; set; }
        public dynamic NamedEntitiesOfTheDayChartData { get; set; }
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
                ? (GetAgendaItemsSortedBySpeechesCount.Last().Item1.Title, GetAgendaItemsSortedBySpeechesCount.ToList().Last().Item2)
                : ("", 0);
        }
        public dynamic GetChartFormattedAgendaItems
        {
            get => GetAgendaItemsSortedBySpeechesCount.Select(t => new { value = t.Item1.Title, count = t.Item2 });
        }


        // ======================================= Sentiments
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
        public dynamic TotalSentimentChartDistribution { get; set; }
        /// <summary>
        /// This holds the data for sentiment charts foreach fraction holding a speech
        /// </summary>
        public List<(string, dynamic)> FractionSentimentCharts { get; set; }

        /// <summary>
        /// This is the namedtity which represents the sentiment the most. If the sentiment ist negativ,
        /// then the ne should have a negative sentiment
        /// </summary>
        public string MostNamedEntityRepresentationOfSentiment {get;set;}

        public SpeechPartViewModel MostCommentedSpeech { get; set; }
        public SpeechPartViewModel MostPositiveSpeech { get; set; }
        public SpeechPartViewModel MostNegativeSpeech { get; set; }

        public List<(Deputy?, int)> AllSpeakers { get; set; }
    }
}
