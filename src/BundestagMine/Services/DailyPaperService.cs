using BundestagMine.Models.Database.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.ViewModels;
using BundestagMine.ViewModels.DailyPaper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace BundestagMine.Services
{
    public class DailyPaperService
    {
        private readonly MetadataService _metadataService;
        private readonly ILogger<DailyPaperService> _logger;
        private readonly BundestagMineDbContext _db;

        public DailyPaperService(BundestagMineDbContext db, 
            ILogger<DailyPaperService> logger,
            MetadataService metadataService)
        {
            _metadataService = metadataService;
            _logger = logger;
            _db = db;
        }

        /// <summary>
        /// Takes in a meetingId and legislaturePeriod and builds the viewmodel data for the paper.
        /// </summary>
        /// <returns></returns>
        public DailyPaperViewModel BuildDailyPaperViewModel(int meetingNumber, int legislaturePeriod)
        {
            try
            {
                var dailyPaperViewModel = new DailyPaperViewModel();

                // Get the protocol
                dailyPaperViewModel.Protocol = _db.Protocols
                    .FirstOrDefault(p => p.LegislaturePeriod == legislaturePeriod && p.Number == meetingNumber);

                // Get the agenda items
                dailyPaperViewModel.AgendaItems = _db.AgendaItems
                    .Where(ag => ag.ProtocolId == dailyPaperViewModel.Protocol.Id)
                    .ToList();

                // Determine the theme of the day. That is the ne with the most occurences in all speeches.
                dailyPaperViewModel.NamedEntityOfTheDay =
                    _db.Speeches
                    .Where(s => s.ProtocolNumber == dailyPaperViewModel.Protocol.Number
                        && s.LegislaturePeriod == dailyPaperViewModel.Protocol.LegislaturePeriod)
                    .AsEnumerable()
                    .SelectMany(s => _db.NamedEntity.Where(ne => ne.NLPSpeechId == s.Id))
                    .GroupBy(ne => ne.LemmaValue)
                    .OrderByDescending(ne => ne.Count())
                    .First()
                    .Key;

                var speech =
                    _db.Speeches
                    .Where(s => s.ProtocolNumber == dailyPaperViewModel.Protocol.Number
                        && s.LegislaturePeriod == dailyPaperViewModel.Protocol.LegislaturePeriod)
                    .AsEnumerable()
                    .OrderByDescending(s => _metadataService.GetActualCommentsAmountOfSpeech(s))
                    .First();
                // Now build the viewmodel from it. This could be done in above statment and just wastes time
                // but since we precalculate the paper anyways, it doesnt matter.
                dailyPaperViewModel.MostCommentedSpeechViewModel = new SpeechViewModel()
                {
                    Speech = speech,
                    Speaker = _db.Deputies.FirstOrDefault(d => d.SpeakerId == speech.SpeakerId),
                    Agenda = _metadataService.GetAgendaItemOfSpeech(speech),
                    ActualCommentsAmount = _metadataService.GetActualCommentsAmountOfSpeech(speech)
                };

                return dailyPaperViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Couldn't build the daily paper view model, error:");
                return null;
            }
        }
    }
}
