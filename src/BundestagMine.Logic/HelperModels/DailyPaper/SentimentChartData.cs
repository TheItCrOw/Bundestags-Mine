using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Logic.HelperModels.DailyPaper
{
    public class SentimentChartData
    {
        /// <summary>
        /// The actuals sentiment
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// pos, neg or neu
        /// </summary>
        public string Value { get; set; }
    }
}
