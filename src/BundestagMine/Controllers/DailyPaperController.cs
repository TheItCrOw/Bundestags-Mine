using BundestagMine.Logic.Services;
using BundestagMine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace BundestagMine.Controllers
{
    [Route("api/DailyPaperController")]
    [ApiController]
    public class DailyPaperController : Controller
    {
        private readonly ILogger<DailyPaperController> _logger;
        private readonly ViewRenderService _viewRenderService;
        private readonly DailyPaperService _dailyPaperService;

        public DailyPaperController(ILogger<DailyPaperController> logger, 
            DailyPaperService dailyPaperService,
            ViewRenderService viewRenderService)
        {
            _viewRenderService = viewRenderService;
            _dailyPaperService = dailyPaperService;
            _logger = logger;
        }


        [HttpGet("/api/DailyPaperController/GetDailyPaperOfProtocol/{meetingAndPeriodNumber}")]
        public async Task<IActionResult> GetDailyPaperOfProtocol(string meetingAndPeriodNumber)
        {
            dynamic response = new ExpandoObject();

            try
            {
                var splited = meetingAndPeriodNumber.Split(",");
                var meetingNumber = int.Parse(splited[0]);
                var legislaturePeriod = int.Parse(splited[1]);

                var dailyPaperViewModel = _dailyPaperService.GetDailyPaperAsViewModel(meetingNumber, legislaturePeriod);

                response.result = await _viewRenderService.RenderToStringAsync("DailyPaper/_DailyPaperView", dailyPaperViewModel);
                response.status = "200";
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't build the daily paper view model";
                _logger.LogError(ex, "Error fetching the daily paper:");
            }

            return Json(response);
        }
    }
}
