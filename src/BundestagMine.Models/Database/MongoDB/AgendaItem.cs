using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.Models.Database.MongoDB
{
    public class AgendaItem : DBEntity
    {
        public string Title { get; set; }

        /// <summary>
        /// This contains HTML.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// This is the actual top number like 7, ZP3 or so
        /// </summary>
        public string AgendaItemNumber { get; set; }

        /// <summary>
        /// This is the order in which they are held.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// I believe we have to use the date to assign the agendaitems to their protocols
        /// since the legislature number is not stored on the scraped page. We therefore store it for savety reasons...
        /// It also contains the time of the day like 10:50
        /// </summary>
        public DateTime Date { get; set; }
        public Guid ProtocolId { get; set; }
    }
}
