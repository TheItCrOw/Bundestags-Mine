using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.Models.Database.MongoDB
{
    public class Deputy : MongoDBEntity
    {
        public string AcademicTitle { get; set; }
        public DateTime HistorySince { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime DeathDate { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public string Religion { get; set; }
        public string Profession { get; set; }
        public string Party { get; set; }
        public string Fraction { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string SpeakerId { get; set; }

        public string GetFullName() => FirstName + " " + LastName;
        public string GetOrga() => Fraction ?? Party ?? "Parteilos";
    }
}
