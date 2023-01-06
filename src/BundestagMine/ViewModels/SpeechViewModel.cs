using BundestagMine.Models.Database.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.ViewModels
{
    public class SpeechViewModel
    {
        public Speech Speech { get; set; }
        public Deputy Speaker { get; set; }
        public int TopicMentionCount { get; set; }
        public AgendaItem Agenda { get; set; }
        public List<string> Topics { get; set; }

        /// <summary>
        /// The amount of comments this speech received without the [BEIFALL] comments.
        /// </summary>
        public int ActualCommentsAmount { get; set; }
    }
}
