using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Logic.HelperModels.DailyPaper
{
    public class PollChartData
    {
        /// <summary>
        /// Yes, No, Abscence etc.
        /// </summary>
        public string PollResult { get; set; }

        /// <summary>
        /// The amount of times this poll result was submitted.
        /// </summary>
        public int Count { get; set; }
    }
}
