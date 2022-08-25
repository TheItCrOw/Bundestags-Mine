using BundestagMine.Models.Database.MongoDB;
using System;
using System.Collections.Generic;

namespace BundestagMine.ViewModels
{
    public class GlobalSearchResultViewModel
    {
        public List<SpeechViewModel> Speeches { get; set; }
        public List<AgendaItemViewModel> AgendaItems { get; set; }
        public List<Deputy> Deputies { get; set; }
        public List<PollViewModel> Polls { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
