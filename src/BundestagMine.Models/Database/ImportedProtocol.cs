using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.Models.Database
{
    /// <summary>
    /// Model for new speeches that are fetched from the bundestag page in JAVA and ready to be analysed
    /// </summary>
    public class ImportedProtocol : DBEntity
    {        
        public DateTime ImportedDate { get; set; }

        /// <summary>
        /// This contains the protocol, agenda items, speeches and deputies.
        /// </summary>
        public string ProtocolJson { get; set; }
    }
}
