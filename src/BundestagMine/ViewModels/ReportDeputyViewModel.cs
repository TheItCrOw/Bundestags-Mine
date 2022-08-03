using BundestagMine.Models.Database.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.ViewModels
{
    public class ReportDeputyViewModel : SpeechStatisticsViewModel
    {
        public Deputy Deputy { get; set; }
    }
}
