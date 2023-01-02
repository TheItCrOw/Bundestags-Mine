using System;
using System.Collections.Generic;

namespace BundestagMine.RequestModels
{
    public class FilterDownloadRequest
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public List<string> Fractions { get; set; }
        public List<string> Parties { get; set; }
        public List<string> ExplicitSpeakers { get; set; }
        public string Email { get; set; }

        public override string ToString()
        {
            return $"From: {From}, To: {To}, Fractions: {string.Join(", ",Fractions?.ToArray())}, " +
                $"Parties: {string.Join(", ", Parties?.ToArray())}, Speakers: {string.Join(", ",ExplicitSpeakers?.ToArray())}";
        }
    }
}
