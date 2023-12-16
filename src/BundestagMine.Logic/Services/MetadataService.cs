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
        /// Gets all agendaitems of a given protocol
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public IEnumerable<AgendaItem> GetAgendaItemsOfProtocol(Protocol protocol) => _db.AgendaItems
            .Where(a => a.ProtocolId == protocol.Id);

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

        public Deputy GetSpeakerOfSpeech(Speech speech) => _db.Deputies.FirstOrDefault(d => d.SpeakerId == speech.SpeakerId);

        /// <summary>
        /// We have speeches that are not assignable to an agendaitem because of the xml issue. 
        /// Fetch these speeches.
        /// </summary>
        /// <param name="period"></param>
        /// <param name="protocolNumber"></param>
        /// <returns></returns>
        public IEnumerable<NLPSpeech> GetUnassignableNLPSpeeches(Protocol protocol)
        {
            if (protocol == default) return new List<NLPSpeech>();

            return _db.NLPSpeeches
                .Where(s => s.LegislaturePeriod == protocol.LegislaturePeriod
                        && s.ProtocolNumber == protocol.Number
                        && !_db.AgendaItems.Any(a => a.ProtocolId == protocol.Id && a.Order == s.AgendaItemNumber));
        }
        public async Task<IEnumerable<NLPSpeech>> GetUnassignableNLPSpeechesAsync(Guid protocolId)
            => GetUnassignableNLPSpeeches(await _db.Protocols.FirstOrDefaultAsync(p => p.Id == protocolId));

        /// <summary>
        /// We have speeches that are not assignable to an agendaitem because of the xml issue. 
        /// Fetch these speeches count.
        /// </summary>
        /// <param name="period"></param>
        /// <param name="protocolNumber"></param>
        /// <returns></returns>
        public async Task<int> GetUnassignableNLPSpeechesCountAsync(Guid protocolId) =>
            (await GetUnassignableNLPSpeechesAsync(protocolId)).Count();

        /// <summary>
        /// Gets all speeches of an agenda items
        /// </summary>
        /// <param name="period"></param>
        /// <param name="protocolNumber"></param>
        /// <param name="agendaNumber">This is the order!</param>
        /// <returns></returns>
        public List<NLPSpeech> GetNLPSpeechesOfAgendaItem(int period, int protocolNumber, int agendaNumber)
        {
            return _db.NLPSpeeches
                .Where(s => s.LegislaturePeriod == period && s.ProtocolNumber == protocolNumber
                        && s.AgendaItemNumber == agendaNumber)
                .ToList();
            // THIS BELOW DOESNT WORK RIGHT NOW. We need a better fix...
            // This is tricky. The xml protocols we parse have different agenda item structure than the 
            // bundestag website. Thus, there are sometimes more or less agenda items in the xml, than on the page.
            // This causes wrong assignment of speeches and sometimes hides speeches because they belong
            // to an agenda item, that does not exist on the page.
            // That is why we only go after the order for now.
            var test = _db.NLPSpeeches
                        .Where(s => s.LegislaturePeriod == period && s.ProtocolNumber == protocolNumber)
                        .OrderBy(s => s.AgendaItemNumber)
                        .AsEnumerable()
                        .GroupBy(s => s.AgendaItemNumber)
                        .Where((g, i) => i == agendaNumber - 1)
                        .SelectMany(g => g.Select(s => s))
                        .ToList();
            return test;
        }

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

        /// <summary>
        /// Gets all polls with their entries of a protocol
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public List<Poll> GetPollsOfProtocol(Protocol protocol) =>
            _db.Polls.Where(p => p.LegislaturePeriod == protocol.LegislaturePeriod && p.ProtocolNumber == protocol.Number)
            .Include(p => p.Entries)
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
        /// Gets all applaud comments of a speech
        /// </summary>
        /// <param name="speech"></param>
        /// <returns></returns>
        public int GetApplaudCommentsAmountOfSpeech(Speech speech) => _db.SpeechSegment
                .Where(ss => ss.SpeechId == speech.Id)
                .Include(ss => ss.Shouts)
                .SelectMany(ss => ss.Shouts.Where(sh => sh.Text.ToLower().Contains("beifall")))
                .Count();

        /// <summary>
        /// Gets all speakers of a protocol meeting
        /// </summary>
        /// <param name="meeting"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public List<Deputy?> GetAllSpeakersOfProtocol(int meeting, int period) => _db.Speeches
            .Where(s => s.LegislaturePeriod == period && s.ProtocolNumber == meeting)
            .Select(s => _db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId))
            .AsEnumerable()
            .Where(s => s != default)
            .DistinctBy(s => s.SpeakerId)
            .ToList();

        /// <summary>
        /// Gets the topics of a speech which are the three most frequent NEs in it.
        /// </summary>
        /// <param name="speech"></param>
        /// <returns></returns>
        public List<string> GetTopicsOfSpeech(Speech speech) =>
            _db.NamedEntity
                        .Where(ne => ne.ShoutId == Guid.Empty && ne.NLPSpeechId == speech.Id)
                        .GroupBy(ne => ne.LemmaValue)
                        .OrderByDescending(ne => ne.Count())
                        .Take(3)
                        .Select(ne => ne.Key)
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
