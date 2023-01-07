using BundestagMine.Models.Database.MongoDB;
using System.Collections.Generic;

namespace BundestagMine.Logic.ViewModels.DailyPaper
{
    public class DailyPaperViewModel
    {
        public Protocol Protocol { get; set; }

        /// <summary>
        /// The ne with the most occurencens in all speeches.
        /// </summary>
        public string NamedEntityOfTheDay{ get; set; }

        public List<AgendaItem> AgendaItems { get; set; }

        /// <summary>
        /// The speech that received the most actual comments.
        /// </summary>
        public SpeechViewModel MostCommentedSpeechViewModel { get; set; }
    }
}
