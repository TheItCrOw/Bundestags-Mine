using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.Models.Database
{
    public class PollEntry : DBEntity
    {
        public Guid PollId { get; set; }
        public string Fraction { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public bool Yes { get; set; }
        public bool No { get; set; }

        public string VoteResultAsGermanString
        {
            get
            {
                if (Yes) return "Ja";
                if (No) return "Nein";
                if (Abstention) return "Enthalten";
                if (NotSubmitted) return "Nicht abg.";
                return "Invalide";
            }
        }

        /// <summary>
        /// Enhaltung
        /// </summary>
        public bool Abstention { get; set; }
        public bool NotValid { get; set; }
        public bool NotSubmitted { get; set; }

        /// <summary>
        /// Bezeichnung. Dont know whats it for tho.
        /// </summary>
        public string Designation { get; set; }
        public string Comment { get; set; }
    }
}
