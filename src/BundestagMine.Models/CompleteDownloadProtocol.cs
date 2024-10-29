using BundestagMine.Models.Database;
using BundestagMine.Models.Database.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Models
{
    public class CompleteDownloadProtocol
    {
        public Protocol Protocol { get; set; }
        public List<AgendaItem> AgendaItems { get; set; }
        public List<NLPSpeech> NLPSpeeches { get; set; }
        public List<Category> SpeechCategories { get; set; }
    }
}
