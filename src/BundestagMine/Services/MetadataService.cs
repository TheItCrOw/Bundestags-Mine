using BundestagMine.Models.Database;
using BundestagMine.Models.Database.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.ViewModels;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.Services
{
    public class MetadataService
    {
        private readonly BundestagMineDbContext _db;

        public MetadataService(BundestagMineDbContext db)
        {
            _db = db;
        }

        public List<dynamic> GetFractions()
        {
            var fractions = new List<dynamic>();
            foreach (var deputy in _db.Deputies.ToList())
            {
                if (!string.IsNullOrEmpty(deputy.Fraction) && !fractions.Any(p => p.id == deputy.Fraction))
                {
                    dynamic fraction = new ExpandoObject();
                    fraction.id = deputy.Fraction;
                    fractions.Add(fraction);
                }
            }
            return fractions;
        }

        public AgendaItem GetAgendaItemOfSpeech(Speech speech) => _db.AgendaItems.FirstOrDefault(a =>
                        a.ProtocolId == _db.Protocols.SingleOrDefault(p =>
                            p.Number == speech.ProtocolNumber && p.LegislaturePeriod == speech.LegislaturePeriod).Id
                        && a.Order == speech.AgendaItemNumber);        
    }
}
