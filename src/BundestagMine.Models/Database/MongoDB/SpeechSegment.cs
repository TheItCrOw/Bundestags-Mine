using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.Models.Database.MongoDB
{
    public class SpeechSegment : DBEntity
    {
        public string Text { get; set; }
        public List<Shout> Shouts { get; set; }
        public Guid SpeechId { get; set; }
    }
}
