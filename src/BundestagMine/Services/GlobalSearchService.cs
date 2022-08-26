using BundestagMine.Models.Database.MongoDB;
using BundestagMine.RequestModels;
using BundestagMine.SqlDatabase;
using BundestagMine.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BundestagMine.Services
{
    public class GlobalSearchService
    {
        private readonly MetadataService _metadataService;
        private readonly BundestagMineDbContext _db;

        public GlobalSearchService(BundestagMineDbContext db, MetadataService metadataService)
        {
            _metadataService = metadataService;
            _db = db;
        }

        /// <summary>
        /// Searches the polls and returns them as PollViewModels
        /// </summary>
        /// <param name="search"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public List<PollViewModel> SearchPolls(string search, DateTime from, DateTime to, int offset = 0, int take = 15)
        {
            return _db.Polls
                    .Where(p => p.Title.ToLower().Contains(search) && p.Date >= from && p.Date <= to)
                    .Select(p => new PollViewModel()
                    {
                        Poll = p,
                        Entries = _db.PollEntries.Where(pe => pe.PollId == p.Id).ToList()
                    })
                    .ToList();
        }

        /// <summary>
        /// Searches the agendaitems and returns them as AgendaItemViewModels
        /// </summary>
        /// <param name="search"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public List<AgendaItemViewModel> SearchAgendaItems(string search, DateTime from, DateTime to, int offset = 0, int take = 15)
        {
            return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                    .SelectMany(p => _db.AgendaItems.Where(a => a.ProtocolId == p.Id && a.Title.ToLower().Contains(search)))
                    .Skip(offset)
                    .Take(take)
                    .Select(a => new AgendaItemViewModel()
                    {
                        AgendaItem = a,
                        Protocol = _db.Protocols.FirstOrDefault(p => p.Id == a.ProtocolId)
                    })
                    .ToList();
        }

        /// <summary>
        /// Searches the speakers and returns them as deputies
        /// </summary>
        /// <param name="search"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public List<Deputy> SearchSpeakers(string search, DateTime from, DateTime to, int offset = 0, int take = 15)
        {
            return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                    .SelectMany(p => _db.Speeches
                         .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod))
                    .SelectMany(s => _db.Deputies.Where(d => d.SpeakerId == s.SpeakerId
                                && string.Concat(d.FirstName ?? "", d.LastName ?? "", d.Fraction ?? "", d.Party ?? "").ToLower().Contains(search)))
                    .Skip(offset)
                    .Take(take)
                    .ToList();
        }

        /// <summary>
        /// Searches the speeches and returns them as speechviewmodels
        /// </summary>
        /// <param name="search"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public List<SpeechViewModel> SearchSpeeches(string search, DateTime from, DateTime to, int offset = 0, int take = 15)
        {
            return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                    .SelectMany(p => _db.Speeches
                        .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && s.Text.ToLower().Contains(search)))
                    .Skip(offset)
                    .Take(take)
                    .Select(s => new SpeechViewModel()
                    {
                        Speech = s,
                        Agenda = _metadataService.GetAgendaItemOfSpeech(s),
                        Speaker = _db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId)
                    })
                    .ToList();
        }
    }
}
