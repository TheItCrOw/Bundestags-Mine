using BundestagMine.Models.Database.MongoDB;
using System.Collections.Generic;

namespace BundestagMine.ViewModels
{
    public class SpeakerInspectorViewModel
    {
        public Deputy Deputy { get; set; }
        public List<SpeechViewModel> Speeches { get; set; }
        public List<PollViewModel> Polls { get; set; }
        public List<SpeechCommentViewModel> Comments { get; set; }

        // These properties, idk yet how to visualize them and what data we need. Will look into that later.
        public List<string> Topics { get; set; }
        public List<Sentiment> Sentiments { get; set; }
    }
}
