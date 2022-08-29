using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.RequestModels
{
    public class GlobalSearchRequest
    {
        public bool SearchSpeeches { get; set; }
        public bool SearchSpeakers { get; set; }
        public bool SearchAgendaItems { get; set; }
        public bool SearchPolls { get; set; }
        public string SearchString { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int Offset { get; set; }
        public int TotalCount { get; set; }
        public int Take { get; set; }

        public override string ToString()
        {
            return $"Speeches: {SearchSpeeches}, Speakers: {SearchSpeakers}, AgendaItems: {SearchAgendaItems}, Polls: {SearchPolls}\n" +
                $"Search: {SearchString}, From: {From}, To: {To}, Offset {Offset}";
        }
    }
}
