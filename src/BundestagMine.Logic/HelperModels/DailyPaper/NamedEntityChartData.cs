using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Logic.HelperModels.DailyPaper
{
    public class NamedEntityChartData
    {
        public string NamedEntity { get; set; }
        public int NamedEntityOccurences { get; set; }
        public List<SentimentChartData> Sentiments { get; set; }
    }
}
