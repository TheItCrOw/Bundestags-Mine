using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.RequestModels
{
    public class TopicAnalysisConfigurationRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> SpeakerIds { get; set; }
        public List<string> Fractions { get; set; }
        public List<string> Parties { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        /// <summary>
        /// Should only be a namedentity lemma value for now.
        /// </summary>
        public string TopicLemmaValue { get; set; }
    }
}
