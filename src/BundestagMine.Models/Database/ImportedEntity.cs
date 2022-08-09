using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.Models.Database
{
    public class ImportedEntity : DBEntity
    {
        public string ImportedDate { get; set; }

        /// <summary>
        /// This contains the protocol, agenda items, speeches and deputies.
        /// </summary>
        public string ModelJson { get; set; }

        public ModelType Type { get; set; }

        /// <summary>
        /// If the model is of type NLPSpeech or deputy, then they belong to a protocol
        /// </summary>
        public Guid ProtocolId { get; set; }
    }

    public enum ModelType
    {
        Protocol,
        NLPSpeech,
        Deputy
    }
}
