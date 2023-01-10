using BundestagMine.Models.Database;
using BundestagMine.Models.Database.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.Logic.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.Logic.Services
{
    public class MetadataService
    {
        private readonly BundestagMineDbContext _db;

        public MetadataService(BundestagMineDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Gets all the speecehs of a protocol
        /// </summary>
        /// <param name="period"></param>
        /// <param name="meetingNumber"></param>
        /// <returns></returns>
        public IEnumerable<Speech> GetSpeechesOfProtocol(int period, int meetingNumber) => _db.Speeches
            .Where(s => s.LegislaturePeriod == period && s.ProtocolNumber == meetingNumber);            

        /// <summary>
        /// Gets the count of speeches in an agendaitem
        /// </summary>
        /// <param name="period"></param>
        /// <param name="protocolNumber"></param>
        /// <param name="agendaNumber"></param>
        /// <returns></returns>
        public int GetSpeechesCountOfAgendaItem(int period, int protocolNumber, int agendaNumber) => _db.Speeches
                    .Where(s => s.LegislaturePeriod == period && s.ProtocolNumber == protocolNumber && s.AgendaItemNumber == agendaNumber)
                    .Count();

        /// <summary>
        /// Gets all speeches of an agenda items
        /// </summary>
        /// <param name="period"></param>
        /// <param name="protocolNumber"></param>
        /// <param name="agendaNumber"></param>
        /// <returns></returns>
        public List<Speech> GetSpeechesOfAgendaItem(int period, int protocolNumber, int agendaNumber) => _db.Speeches
                    .Where(s => s.LegislaturePeriod == period && s.ProtocolNumber == protocolNumber && s.AgendaItemNumber == agendaNumber)
                    .ToList();

        /// <summary>
        /// Gets all parties
        /// </summary>
        /// <returns></returns>
        public List<string> GetPartiesAsStrings() => _db.Deputies.Select(d => d.Party).Distinct().ToList();

        /// <summary>
        /// Gets all fractions as strings
        /// </summary>
        /// <returns></returns>
        public List<string> GetFractionsAsStrings() => _db.Deputies.Select(d => d.Fraction).Distinct().ToList();

        /// <summary>
        /// Gets all fractions of a meeting. 
        /// </summary>
        /// <param name="period"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public List<string> GetFractionsOfProtocol(int period, int number) => _db.Speeches
            .Where(p => p.LegislaturePeriod == period && p.ProtocolNumber == number)
            .Select(s => _db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId))
            .Select(s => s == null ? "" : s.Fraction)
            .Where(f => !string.IsNullOrEmpty(f))
            .Distinct()
            .ToList();


        // Old method, it sucks, I know.
        // =============================================================
        /// <summary>
        /// Gets all fractions
        /// </summary>
        /// <returns></returns>
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
        // =============================================================

        /// <summary>
        /// Determines the amount of actual comments a speech received
        /// </summary>
        /// <param name="speech"></param>
        /// <returns></returns>
        public int GetActualCommentsAmountOfSpeech(Speech speech) => _db.SpeechSegment
                .Where(ss => ss.SpeechId == speech.Id)
                .Include(ss => ss.Shouts)
                .AsEnumerable()
                .Sum(ss => ss.Shouts.Where(sh => !sh.Text.ToLower().Contains("beifall")).Count());

        /// <summary>
        /// Gets all actual comments of a speech
        /// </summary>
        /// <param name="speech"></param>
        /// <returns></returns>
        public List<Shout> GetActualCommentsOfSpeech(Speech speech) => _db.SpeechSegment
                .Where(ss => ss.SpeechId == speech.Id)
                .Include(ss => ss.Shouts)
                .SelectMany(ss => ss.Shouts.Where(sh => !sh.Text.ToLower().Contains("beifall")))
                .ToList();

        /// <summary>
        /// Gets all applaud comments of a speech
        /// </summary>
        /// <param name="speech"></param>
        /// <returns></returns>
        public List<Shout> GetApplaudCommentsOfSpeech(Speech speech) => _db.SpeechSegment
                .Where(ss => ss.SpeechId == speech.Id)
                .Include(ss => ss.Shouts)
                .SelectMany(ss => ss.Shouts.Where(sh => sh.Text.ToLower().Contains("beifall")))
                .ToList();

        /// <summary>
        /// Gets all speakers of a protocol meeting
        /// </summary>
        /// <param name="meeting"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public List<Deputy?> GetAllSpeakersOfProtocol(int meeting, int period) => _db.Speeches
            .Where(s => s.LegislaturePeriod == period && s.ProtocolNumber == meeting)
            .Select(s => _db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId))
            .ToList();

        /// <summary>
        /// Gets the agendaitem of a speech
        /// </summary>
        /// <param name="speech"></param>
        /// <returns></returns>
        public AgendaItem? GetAgendaItemOfSpeech(Speech speech) => _db.AgendaItems.FirstOrDefault(a =>
                        a.ProtocolId == _db.Protocols.SingleOrDefault(p =>
                            p.Number == speech.ProtocolNumber && p.LegislaturePeriod == speech.LegislaturePeriod).Id
                        && a.Order == speech.AgendaItemNumber);        
    }
}
