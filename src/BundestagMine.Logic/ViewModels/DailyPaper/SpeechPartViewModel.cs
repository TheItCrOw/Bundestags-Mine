using BundestagMine.Models.Database.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Logic.ViewModels.DailyPaper
{
    public class SpeechPartViewModel
    {
        public Speech Speech { get; set; }
        public AgendaItem AgendaItem { get; set; }
        public double Sentiment { get; set; }
        public int ActualCommentsAmount { get; set; }
        public Deputy Speaker { get; set; }
        public List<string> MostTwoUsedNamedEntity { get; set; }
        public SpeechPartType SpeechPartType { get; set; }
    }

    public enum SpeechPartType
    {
        MostCommented,
        MostPositive,
        MostNegative
    }
}
