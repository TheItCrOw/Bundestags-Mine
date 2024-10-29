using BundestagMine.Models.Database.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.Logic.ViewModels
{
    public class TopicAnalysisReportViewModel
    {
        public string ReportId { get; set; }
        public string Name { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        /// <summary>
        /// id, name, type
        /// </summary>
        public List<(string, string, string)> Entities { get; set; }
        public string Topic { get; set; }
    }
}
