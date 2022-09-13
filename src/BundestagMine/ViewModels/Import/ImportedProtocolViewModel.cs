using System;
using System.Collections.Generic;

namespace BundestagMine.ViewModels.Import
{
    public class ImportedProtocolViewModel
    {
        public DateTime ImportedDate { get; set; }
        public string ProtocolJson { get; set; }
        public Guid ImportedEntityId { get; set; }
        public List<Guid> SpeechIds { get; set; }
    }
}
