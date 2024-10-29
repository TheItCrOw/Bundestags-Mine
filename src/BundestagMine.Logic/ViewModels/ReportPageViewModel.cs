using BundestagMine.Models.Database.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.Logic.ViewModels
{
    /// <summary>
    /// The '..Graph' properties hold a string which can be passed into a chart.js chart. We render the charts that way
    /// </summary>
    public class ReportPageViewModel
    {
        public Guid Id { get; set; }
        public int PageNumber { get; set; }
        public string ReportId { get; set; }
        public string Topic { get; set; }
        public object SentimentGraphData { get; set; }
        public SpeechStatisticsViewModel StatisticsViewModel { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public List<SpeechViewModel> TopicSpeeches { get; set; }
        public List<SpeechCommentViewModel> TopicCommentsFromEntity { get; set; }
        public object TopicToOtherTopicsCompareGraphData { get; set; }
        public List<PollViewModel> TopicPolls { get; set; }
    }
}
