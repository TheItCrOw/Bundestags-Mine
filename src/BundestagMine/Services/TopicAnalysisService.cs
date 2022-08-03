using BundestagMine.Models.Database;
using BundestagMine.Models.Database.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.Utility;
using BundestagMine.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.Services
{
    public class TopicAnalysisService
    {
        private readonly BundestagScraperService _bundestagScraperService;
        private readonly AnnotationService _annotationService;
        private readonly BundestagMineDbContext _db;

        public TopicAnalysisService(BundestagMineDbContext db,
            AnnotationService annotationService,
            BundestagScraperService bundestagScraperService)
        {
            _bundestagScraperService = bundestagScraperService;
            _annotationService = annotationService;
            _db = db;
        }

        /// <summary>
        /// Fetches all polls which contain the given topic
        /// </summary>
        /// <returns></returns>
        public List<PollViewModel> GetPollsOfTopic(int limit,
            string topic, 
            DateTime from, 
            DateTime to,
            string fraction,
            string party,
            string deputyId)
        {
            Deputy deputy = null;
            if (!string.IsNullOrEmpty(deputyId)) deputy = _db.Deputies.FirstOrDefault(d => d.Id.ToString() == deputyId);

            // Return the polls which have the topic and return those entries, which we fit the criteria.
            return _db.Polls
                .Where(p => p.Title.Contains(topic))
                .Select(p => new PollViewModel() 
                {
                    Poll = p,
                    Entries = _db.PollEntries
                        .Where(pe => pe.PollId == p.Id && (
                            (fraction != string.Empty && NameHelper.GetAliasesOf(fraction).Contains(pe.Fraction)) ||
                            (deputy != null && pe.FirstName + pe.LastName == deputy.FirstName + deputy.LastName)))
                        .Take(30)
                        .ToList()
                })
                .Where(p => p.Entries.Count > 0)
                .Take(limit)
                .ToList();
        }

        /// <summary>
        /// Builds the deputy view model for a report page.
        /// </summary>
        /// <param name="deputyId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="topic"></param>
        /// <returns></returns>
        public ReportDeputyViewModel BuildDeputyViewModel(Deputy deputy, DateTime from, DateTime to, string topic)
        {
            var totalProtocolsAmount = (Decimal)_db.Protocols.Count(); ;
            var totalProtocolsAmountTimeFramed = (Decimal)_db.Protocols.Where(p => p.Date >= from && p.Date <= to).Count();

            var deputyVm = new ReportDeputyViewModel()
            {
                Deputy = deputy,
                EntityName = deputy.FirstName + " " + deputy.LastName,

                TotalSpeechesAmount = _db.Speeches.Where(s => s.SpeakerId == deputy.SpeakerId).Count(),
                TotalSpeechesAmountTimeFramed = _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                    .SelectMany(p => _db.Speeches
                        .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod && s.SpeakerId == deputy.SpeakerId))
                    .Count(),

                TotalSpeechesAmountTopic = _db.Speeches.Where(s => s.SpeakerId == deputy.SpeakerId
                    && _db.NamedEntity.FirstOrDefault(ne => ne.NLPSpeechId == s.Id && ne.LemmaValue == topic) != null).Count(),
                TotalSpeechesAmountTopicTimeFramed = _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                    .SelectMany(p => _db.Speeches
                        .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && s.SpeakerId == deputy.SpeakerId && _db.NamedEntity.FirstOrDefault(ne => ne.NLPSpeechId == s.Id && ne.LemmaValue == topic) != null))
                    .Count(),

                PortraitUrl = _bundestagScraperService.GetDeputyPortraitFromImageDatabase(deputy)
            };

            deputyVm.AverageSpeechesAmount = (Decimal)deputyVm.TotalSpeechesAmount / totalProtocolsAmount;
            deputyVm.AverageSpeechesAmountTimeFramed =
                (Decimal)deputyVm.TotalSpeechesAmountTimeFramed / totalProtocolsAmountTimeFramed;

            deputyVm.AverageSpeechesAmountTopic = (Decimal)deputyVm.TotalSpeechesAmountTopic / (Decimal)totalProtocolsAmount;
            deputyVm.AverageSpeechesAmountTopicTimeFramed =
                (Decimal)deputyVm.TotalSpeechesAmountTopicTimeFramed / (Decimal)totalProtocolsAmountTimeFramed;

            return deputyVm;
        }

        /// <summary>
        /// Builds the fraction view model for a report page.
        /// </summary>
        /// <param name="deputyId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="topic"></param>
        /// <returns></returns>
        public ReportFractionViewModel BuildFractionViewModel(string fraction, DateTime from, DateTime to, string topic)
        {
            var totalProtocolsAmount = (Decimal)_db.Protocols.Count(); ;
            var totalProtocolsAmountTimeFramed = (Decimal)_db.Protocols.Where(p => p.Date >= from && p.Date <= to).Count();

            var fractionVm = new ReportFractionViewModel()
            {
                EntityName = fraction,

                TotalSpeechesAmount = _db.Speeches.Where(s => 
                    _db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId && d.Fraction == fraction) != null).Count(),
                TotalSpeechesAmountTimeFramed = _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                    .SelectMany(p => _db.Speeches
                        .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod &&
                        _db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId && d.Fraction == fraction) != null))
                    .Count(),

                TotalSpeechesAmountTopic = _db.Speeches.Where(s => _db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId && d.Fraction == fraction) != null
                    && _db.NamedEntity.FirstOrDefault(ne => ne.NLPSpeechId == s.Id && ne.LemmaValue == topic) != null).Count(),
                TotalSpeechesAmountTopicTimeFramed = _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                    .SelectMany(p => _db.Speeches
                        .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && _db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId && d.Fraction == fraction) != null
                            && _db.NamedEntity.FirstOrDefault(ne => ne.NLPSpeechId == s.Id && ne.LemmaValue == topic) != null))
                    .Count(),

                PortraitUrl = ""
            };

            fractionVm.AverageSpeechesAmount = (Decimal)fractionVm.TotalSpeechesAmount / totalProtocolsAmount;
            fractionVm.AverageSpeechesAmountTimeFramed =
                (Decimal)fractionVm.TotalSpeechesAmountTimeFramed / totalProtocolsAmountTimeFramed;

            fractionVm.AverageSpeechesAmountTopic = (Decimal)fractionVm.TotalSpeechesAmountTopic / (Decimal)totalProtocolsAmount;
            fractionVm.AverageSpeechesAmountTopicTimeFramed =
                (Decimal)fractionVm.TotalSpeechesAmountTopicTimeFramed / (Decimal)totalProtocolsAmountTimeFramed;

            return fractionVm;
        }

        /// <summary>
        /// Builds the party view model for a report page.
        /// </summary>
        /// <param name="deputyId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="topic"></param>
        /// <returns></returns>
        public ReportPartyViewModel BuildPartyViewModel(string party, DateTime from, DateTime to, string topic)
        {
            var totalProtocolsAmount = (Decimal)_db.Protocols.Count(); ;
            var totalProtocolsAmountTimeFramed = (Decimal)_db.Protocols.Where(p => p.Date >= from && p.Date <= to).Count();

            var partyVm = new ReportPartyViewModel()
            {
                EntityName = party,

                TotalSpeechesAmount = _db.Speeches.Where(s =>
                    _db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId && d.Party == party) != null).Count(),
                TotalSpeechesAmountTimeFramed = _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                    .SelectMany(p => _db.Speeches
                        .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod &&
                        _db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId && d.Party == party) != null))
                    .Count(),

                TotalSpeechesAmountTopic = _db.Speeches.Where(s => _db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId && d.Party == party) != null
                    && _db.NamedEntity.FirstOrDefault(ne => ne.NLPSpeechId == s.Id && ne.LemmaValue == topic) != null).Count(),
                TotalSpeechesAmountTopicTimeFramed = _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                    .SelectMany(p => _db.Speeches
                        .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && _db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId && d.Party == party) != null
                            && _db.NamedEntity.FirstOrDefault(ne => ne.NLPSpeechId == s.Id && ne.LemmaValue == topic) != null))
                    .Count(),

                PortraitUrl = ""
            };

            partyVm.AverageSpeechesAmount = (Decimal)partyVm.TotalSpeechesAmount / totalProtocolsAmount;
            partyVm.AverageSpeechesAmountTimeFramed =
                (Decimal)partyVm.TotalSpeechesAmountTimeFramed / totalProtocolsAmountTimeFramed;

            partyVm.AverageSpeechesAmountTopic = (Decimal)partyVm.TotalSpeechesAmountTopic / (Decimal)totalProtocolsAmount;
            partyVm.AverageSpeechesAmountTopicTimeFramed =
                (Decimal)partyVm.TotalSpeechesAmountTopicTimeFramed / (Decimal)totalProtocolsAmountTimeFramed;

            return partyVm;
        }
    }
}
