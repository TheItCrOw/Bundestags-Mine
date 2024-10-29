using BundestagMine.Logic.Services;
using BundestagMine.RequestModels;
using BundestagMine.Services;
using BundestagMine.SqlDatabase;
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

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("/api/GlobalSearchController/GetPollViewModelListViewOfSpeaker/{speakerId}")]
        public async Task<IActionResult> GetPollViewModelListViewOfSpeaker(string speakerId)
        {
            dynamic response = new ExpandoObject();

            try
            {
                response.status = "200";
                var pollList = _globalSearchService.GetPollViewModelsOfSpeaker(speakerId);
                response.result = await _viewRenderService.RenderToStringAsync("_PollViewModelListView", pollList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching polls:");
                response.status = "400";
                response.message = "Couldn't fetch polls, error in logs";
            }

            return Json(response);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("/api/GlobalSearchController/GetSpeechCommentViewModelListViewOfSpeaker/{speakerId}")]
        public async Task<IActionResult> GetSpeechCommentViewModelListViewOfSpeaker(string speakerId)
        {
            dynamic response = new ExpandoObject();

            try
            {
                response.status = "200";
                var commentList = _globalSearchService.GetSpeechCommentViewModelsOfSpeaker(speakerId);
                response.result = await _viewRenderService.RenderToStringAsync("_SpeechCommentViewModelListView", commentList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching comments:");
                response.status = "400";
                response.message = "Couldn't fetch comments, error in logs";
            }

            return Json(response);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("/api/GlobalSearchController/GetSpeechViewModelListViewOfSpeaker/{speakerId}")]
        public async Task<IActionResult> GetSpeechViewModelListViewOfSpeaker(string speakerId)
        {
            dynamic response = new ExpandoObject();

            try
            {
                response.status = "200";
                var speechesList = _globalSearchService.GetSpeechViewModelsOfSpeaker(speakerId);
                response.result = await _viewRenderService.RenderToStringAsync("_SpeechViewModelListView", speechesList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching speeches:");
                response.status = "400";
                response.message = "Couldn't fetch speeches, error in logs";
            }

            return Json(response);
        }

        /// <summary>
        /// Gets the fully rendered SpeakerInspector html view of a speaker.
        /// </summary>
        /// <param name="speakerId">Not the GUID, but the speakerId of a speaker.</param>
        /// <returns></returns>
        [HttpGet("/api/GlobalSearchController/GetSpeakerInspectorView/{speakerId}")]
        public async Task<IActionResult> GetSpeakerInspectorView(string speakerId)
        {
            dynamic response = new ExpandoObject();

            try
            {
                response.status = "200";
                var inspectorViewModel = _globalSearchService.BuildSpeakerInspectorViewModel(speakerId);
                response.result = await _viewRenderService.RenderToStringAsync("_SpeakerInspectorView", inspectorViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building speaker inspector:");
                response.status = "400";
                response.message = "Couldn't build speaker inspector, error in logs";
            }

            return Json(response);
        }

        /// <summary>
        /// Returns a fully rendered GlobalSearchView of the parameters.
        /// </summary>
        /// <param name="globalSearchRequest"></param>
        /// <returns></returns>
        [HttpPost("/api/GlobalSearchController/GlobalSearch/")]
        public async Task<IActionResult> GlobalSearch(GlobalSearchRequest globalSearchRequest)
        {
            dynamic response = new ExpandoObject();

            try
            {
                response.status = "200";
                object data = null;

                // Build the data viewmodel, then render the view with the viewrenderservice and send back the html.
                if (globalSearchRequest.SearchSpeeches)
                {
                    data = _globalSearchService.SearchSpeeches(globalSearchRequest.SearchString,
                        globalSearchRequest.From, globalSearchRequest.To, globalSearchRequest.TotalCount,
                        globalSearchRequest.Offset, globalSearchRequest.Take);
                }
                else if (globalSearchRequest.SearchShouts)
                {
                    data = _globalSearchService.SearchShouts(globalSearchRequest.SearchString,
                        globalSearchRequest.From, globalSearchRequest.To, globalSearchRequest.TotalCount,
                        globalSearchRequest.Offset, globalSearchRequest.Take);
                }
                else if (globalSearchRequest.SearchSpeakers)
                {
                    data = _globalSearchService.SearchSpeakers(globalSearchRequest.SearchString,
                        globalSearchRequest.From, globalSearchRequest.To, globalSearchRequest.TotalCount,
                        globalSearchRequest.Offset, globalSearchRequest.Take);
                }
                else if (globalSearchRequest.SearchAgendaItems)
                {
                    data = _globalSearchService.SearchAgendaItems(globalSearchRequest.SearchString,
                        globalSearchRequest.From, globalSearchRequest.To, globalSearchRequest.TotalCount,
                        globalSearchRequest.Offset, globalSearchRequest.Take);
                }
                else if (globalSearchRequest.SearchPolls)
                {
                    data = _globalSearchService.SearchPolls(globalSearchRequest.SearchString,
                        globalSearchRequest.From, globalSearchRequest.To, globalSearchRequest.TotalCount,
                        globalSearchRequest.Offset, globalSearchRequest.Take);
                }

                if (data != null)
                    response.result = await _viewRenderService.RenderToStringAsync("GlobalSearch/Results/_GlobalSearchResultView", data);
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't global search, error in logs";
                _logger.LogError(ex, $"Error global searching with request: {globalSearchRequest}");
            }

            return Json(response);
        }
    }
}
