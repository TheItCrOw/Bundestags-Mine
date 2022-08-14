using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.Models.Database.MongoDB
{
    public class Shout : DBEntity
    {
        public string Text { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Fraction { get; set; }
        public string Party { get; set; }
        public string SpeakerId { get; set; }
        public Guid SpeechSegmentId { get; set; }
    }
}
