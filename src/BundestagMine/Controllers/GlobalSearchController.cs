using BundestagMine.RequestModels;
using BundestagMine.Services;
using BundestagMine.SqlDatabase;
using BundestagMine.ViewModels.GlobalSearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace BundestagMine.Controllers
{
    [Route("api/GlobalSearchController")]
    [ApiController]
    public class GlobalSearchController : Controller
    {
        private readonly ViewRenderService _viewRenderService;
        private readonly GlobalSearchService _globalSearchService;
        private readonly ILogger<GlobalSearchController> _logger;
        private readonly BundestagMineDbContext _db;

        public GlobalSearchController(BundestagMineDbContext db, 
            ILogger<GlobalSearchController> logger,
            GlobalSearchService globalSearchService,
            ViewRenderService viewRenderService)
        {
            _viewRenderService = viewRenderService;
            _globalSearchService = globalSearchService;
            _logger = logger;
            _db = db;
        }

        [HttpPost("/api/GlobalSearchController/GlobalSearch/")]
        public async Task<IActionResult> GlobalSearch(GlobalSearchRequest globalSearchRequest)
        {
            dynamic response = new ExpandoObject();

            try
            {
                response.status = "200";

                // Build the data viewmodel, then render the view with the viewrenderservice and send back the html.
                if(globalSearchRequest.SearchSpeeches)
                {
                    var data = _globalSearchService.SearchSpeeches(globalSearchRequest.SearchString,
                        globalSearchRequest.From, globalSearchRequest.To, globalSearchRequest.TotalCount, 
                        globalSearchRequest.Offset, globalSearchRequest.Take);
                    response.result = await _viewRenderService.RenderToStringAsync("GlobalSearch/Results/_GlobalSearchResultView", data);
                }
                else if (globalSearchRequest.SearchSpeakers)
                {
                    var data = _globalSearchService.SearchSpeakers(globalSearchRequest.SearchString,
                        globalSearchRequest.From, globalSearchRequest.To, globalSearchRequest.TotalCount,
                        globalSearchRequest.Offset, globalSearchRequest.Take);
                    response.result = await _viewRenderService.RenderToStringAsync("GlobalSearch/Results/_GlobalSearchResultView", data);
                }
                else if (globalSearchRequest.SearchAgendaItems)
                {
                    var data = _globalSearchService.SearchSpeakers(globalSearchRequest.SearchString.ToLower(),
                        globalSearchRequest.From, globalSearchRequest.To, globalSearchRequest.Offset);
                }
                else if (globalSearchRequest.SearchPolls)
                {
                    var data = _globalSearchService.SearchSpeakers(globalSearchRequest.SearchString.ToLower(),
                        globalSearchRequest.From, globalSearchRequest.To, globalSearchRequest.Offset);
                }
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't global search, error in logs";
                _logger.LogError(ex, $"Error global searching with request: {globalSearchRequest.ToString()}");
            }

            return Json(response);
        }
    }
}
