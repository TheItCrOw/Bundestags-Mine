using BundestagMine.Logic.HelperModels.DailyPaper;
using BundestagMine.Models.Database.MongoDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Logic.ViewModels.FulltextAnalysis
{
    public class NLPSpeechStatisticsViewModel
    {
        public NLPSpeech Speech { get; set; }
        public List<NamedEntityChartData> StackedNamedEntityWithSentimentChartData { get; set; }
        public string FirstTopic => StackedNamedEntityWithSentimentChartData.Count > 0
            ? StackedNamedEntityWithSentimentChartData[0].NamedEntity : "/";
        public string SecondTopic => StackedNamedEntityWithSentimentChartData.Count > 1
            ? StackedNamedEntityWithSentimentChartData[1].NamedEntity : "/";
        public string ThirdTopic => StackedNamedEntityWithSentimentChartData.Count > 2
            ? StackedNamedEntityWithSentimentChartData[2].NamedEntity : "/";

        public dynamic SentimentRadarChartData { get; set; }
        public double AverageSentiment { get; set; }
    }
}
