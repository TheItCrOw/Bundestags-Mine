using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Models.Database
{
    public class DailyPaperSubscription : DBEntity
    {
        public string Email { get; set; }
        public bool Active { get; set; }
        public DateTime InitialSubscriptionDate { get; set; }

        /// <summary>
        /// This is the date the last email was sent to the user
        /// </summary>
        public DateTime LastSendTime { get; set; } 
        public Guid LastSentDailyPaperId { get; set; }

        public string GetStatus() => Active ? "Aktiv" : "Deaktiviert";
    }
}
