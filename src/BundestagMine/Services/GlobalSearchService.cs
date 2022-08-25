using BundestagMine.RequestModels;
using BundestagMine.SqlDatabase;
using BundestagMine.ViewModels;
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
        /// Builds a result viewmodel from a request model for te global search
        /// </summary>
        /// <param name="globalSearchRequest"></param>
        /// <returns></returns>
        public GlobalSearchResultViewModel BuildGlobalSearchResultViewModel(GlobalSearchRequest globalSearchRequest)
        {
            var result = new GlobalSearchResultViewModel();
            var from = globalSearchRequest.From;
            var to = globalSearchRequest.To;
            var search = globalSearchRequest.SearchString.ToLower();

            // Fetch the different entities if needed in the request
            if (globalSearchRequest.IncludeSpeeches)
            {
                result.Speeches = _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                        && s.Text.ToLower().Contains(search)))
                .Select(s => new SpeechViewModel()
                {
                    Speech = s,
                    Agenda = _metadataService.GetAgendaItemOfSpeech(s),
                    Speaker = _db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId)
                })
                .ToList();
            }

            if(globalSearchRequest.IncludeSpeakers)
            {

            }

            return result;
        }
    }
}
