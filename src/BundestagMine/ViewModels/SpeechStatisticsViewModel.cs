using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.ViewModels
{
    public class SpeechStatisticsViewModel
    {
        public string PortraitUrl { get; set; }
        public string EntityName { get; set; }

        /// <summary>
        ///  How many speeches in total
        /// </summary>
        public int TotalSpeechesAmount { get; set; }
        public int TotalSpeechesAmountTimeFramed { get; set; }

        /// <summary>
        /// How many speeches contained the topic at least once
        /// </summary>
        public int TotalSpeechesAmountTopic { get; set; }
        public int TotalSpeechesAmountTopicTimeFramed { get; set; }

        /// <summary>
        /// How many average speeches per protocol
        /// </summary>
        public decimal AverageSpeechesAmount { get; set; }
        public decimal AverageSpeechesAmountTimeFramed { get; set; }

        /// <summary>
        /// How many average speeches per protocol
        /// </summary>
        public decimal AverageSpeechesAmountTopic { get; set; }
        public decimal AverageSpeechesAmountTopicTimeFramed { get; set; }
    }
}
