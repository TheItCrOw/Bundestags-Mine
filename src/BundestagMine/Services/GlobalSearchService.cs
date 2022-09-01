using BundestagMine.Models.Database.MongoDB;
using BundestagMine.RequestModels;
using BundestagMine.SqlDatabase;
using BundestagMine.ViewModels;
using BundestagMine.ViewModels.GlobalSearch;
using Microsoft.EntityFrameworkCore;
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
        /// Searches the shouts and returns them as <see cref="Poll"/>
        /// </summary>
        /// <param name="search"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public GlobalSearchResultViewModel SearchShouts(string search,
            DateTime from,
            DateTime to,
            int totalCount = -1,
            int offset = 0,
            int take = 20)
        {
            return new GlobalSearchResultViewModel()
            {
                ResultList = _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                    .SelectMany(p => _db.Speeches.Include(s => s.Segments).ThenInclude(s => s.Shouts)
                        .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && s.Segments.Any(ss => ss.Shouts.Any(sh => sh.Text.ToLower().Contains(search.ToLower())))))
                    .Skip(offset)
                    .Take(take)
                    .AsEnumerable()
                    .SelectMany(speech =>
                    {
                        var speechSegment = speech.Segments
                            .Where(ss => ss.Shouts.Any(sh => sh.Text.ToLower().Contains(search.ToLower())));

                        var speaker = _db.Deputies.FirstOrDefault(d => d.SpeakerId == speech.SpeakerId);

                        return speechSegment.Where(ss => ss != null).Select(ss => new SpeechCommentViewModel()
                        {
                            Speaker = speaker,
                            SpeechId = speech.Id,
                            SpeechSegment = ss
                        });
                    })
                    .ToList(),
                // Get the total count, but only, if we didnt fetch it already.
                // If its -1, then we need to fetch it. Otherwise, we got it already and avoid the request.
                TotalResults = totalCount == -1 ?
                    _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                    .SelectMany(p => _db.Speeches.Where(s => s.LegislaturePeriod == p.LegislaturePeriod && s.ProtocolNumber == p.Number))
                    .SelectMany(s => _db.SpeechSegment.Where(ss => ss.SpeechId == s.Id && ss.Shouts.Any(sh => sh.Text.ToLower().Contains(search.ToLower()))))
                    .Count()
                : totalCount,
                CurrentPage = offset,
                TakeResults = take,
                SearchString = search,
                Type = ResultType.Shouts
            };
        }

        /// <summary>
        /// Searches the polls and returns them as <see cref="Poll"/>
        /// </summary>
        /// <param name="search"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public GlobalSearchResultViewModel SearchPolls(string search,
            DateTime from,
            DateTime to,
            int totalCount = -1,
            int offset = 0,
            int take = 20)
        {
            return new GlobalSearchResultViewModel()
            {
                ResultList = _db.Polls
                    .Where(p => p.Title.ToLower().Contains(search.ToLower()) && p.Date >= from && p.Date <= to)
                    .Skip(offset)
                    .Take(take)
                    .ToList(),
                // Get the total count, but only, if we didnt fetch it already.
                // If its -1, then we need to fetch it. Otherwise, we got it already and avoid the request.
                TotalResults = totalCount == -1 ?
                    _db.Polls
                    .Where(p => p.Title.ToLower().Contains(search.ToLower()) && p.Date >= from && p.Date <= to)
                    .Count()
                : totalCount,
                CurrentPage = offset,
                TakeResults = take,
                SearchString = search,
                Type = ResultType.Polls
            };
        }

        /// <summary>
        /// Searches the agendaitems and returns them as AgendaItemViewModels
        /// </summary>
        /// <param name="search"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public GlobalSearchResultViewModel SearchAgendaItems(string search,
            DateTime from,
            DateTime to,
            int totalCount = -1,
            int offset = 0,
            int take = 20)
        {
            return new GlobalSearchResultViewModel()
            {
                ResultList = _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                    .SelectMany(p => _db.AgendaItems.Where(a => a.ProtocolId == p.Id && a.Title.ToLower().Contains(search.ToLower())))
                    .Skip(offset)
                    .Take(take)
                    .Select(a => new AgendaItemViewModel()
                    {
                        AgendaItem = a,
                        Protocol = _db.Protocols.FirstOrDefault(p => p.Id == a.ProtocolId)
                    })
                    .ToList(),
                // Get the total count, but only, if we didnt fetch it already.
                // If its -1, then we need to fetch it. Otherwise, we got it already and avoid the request.
                TotalResults = totalCount == -1 ?
                    _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                    .SelectMany(p => _db.AgendaItems.Where(a => a.ProtocolId == p.Id && a.Title.ToLower().Contains(search.ToLower())))
                    .Count()
                : totalCount,
                CurrentPage = offset,
                TakeResults = take,
                SearchString = search,
                Type = ResultType.AgendaItems
            };
        }

        /// <summary>
        /// Searches the speakers and returns them as deputies
        /// </summary>
        /// <param name="search"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public GlobalSearchResultViewModel SearchSpeakers(string search,
            DateTime from,
            DateTime to,
            int totalCount = -1,
            int offset = 0,
            int take = 20)
        {
            return new GlobalSearchResultViewModel()
            {
                ResultList =
                _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                    .SelectMany(p => _db.Speeches
                         .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod))
                    .SelectMany(s => _db.Deputies.Where(d => d.SpeakerId == s.SpeakerId
                                && string.Concat(d.FirstName ?? "", d.LastName ?? "", d.Fraction ?? "", d.Party ?? "").ToLower().Contains(search.ToLower())))
                    .AsEnumerable()
                    .DistinctBy(s => s.SpeakerId)
                    .Skip(offset)
                    .Take(take)
                    .ToList(),
                // Get the total count, but only, if we didnt fetch it already.
                // If its -1, then we need to fetch it. Otherwise, we got it already and avoid the request.
                TotalResults = totalCount == -1 ?
                    _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                    .SelectMany(p => _db.Speeches
                         .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod))
                    .SelectMany(s => _db.Deputies.Where(d => d.SpeakerId == s.SpeakerId
                                && string.Concat(d.FirstName ?? "", d.LastName ?? "", d.Fraction ?? "", d.Party ?? "").ToLower().Contains(search.ToLower())))
                    .AsEnumerable()
                    .DistinctBy(s => s.SpeakerId)
                    .Count()
                : totalCount,
                CurrentPage = offset,
                TakeResults = take,
                SearchString = search,
                Type = ResultType.Speakers
            };
        }

        /// <summary>
        /// Searches the speeches and returns them as a <see cref="GlobalSearchResultViewModel"/>
        /// </summary>
        /// <param name="search"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public GlobalSearchResultViewModel SearchSpeeches(string search,
            DateTime from,
            DateTime to,
            int totalCount = -1,
            int offset = 0,
            int take = 20)
        {
            // I couldnt get it working to make this into one request... So Item1 is the actual speech list and item2 the total speeches count found
            return new GlobalSearchResultViewModel()
            {
                ResultList = // Get the TAKE AMOUNT of speeches
                _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                    .SelectMany(p => _db.Speeches
                        .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && s.Text.ToLower().Contains(search.ToLower())))
                    .Include(s => s.Segments.Where(se => se.Text.ToLower().Contains(search.ToLower())))
                    .Skip(offset * take)
                    .Take(take)
                    .Select(s => new SpeechViewModel()
                    {
                        Speech = s,
                        Agenda = _metadataService.GetAgendaItemOfSpeech(s),
                        Speaker = _db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId)
                    })
                    .ToList(),
                // Get the total count, but only, if we didnt fetch it already.
                // If its -1, then we need to fetch it. Otherwise, we got it already and avoid the request.
                TotalResults = totalCount == -1 ?
                    _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                    .SelectMany(p => _db.Speeches
                        .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && s.Text.ToLower().Contains(search.ToLower())))
                    .Count()
                : totalCount,
                CurrentPage = offset,
                TakeResults = take,
                SearchString = search,
                Type = ResultType.Speeches
            };
        }
    }
}
