using BundestagMine.Logic.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace BundestagMine.Controllers
{
    public class RequestLogLine
    {
        public string LogFilePath { get; set; }
        public int LineCount { get; set; }
    }

    [Route("api/AdminCockpitController")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    public class AdminCockpitController : Controller
    {
        private readonly ILogger<AdminCockpitController> _logger;
        private readonly ImportService _importService;

        public AdminCockpitController(ImportService importService, ILogger<AdminCockpitController> logger)
        {
            _logger = logger;
            _importService = importService;
        }

        [HttpPost("/api/AdminCockpitController/PollImportLogLines/")]
        public IActionResult PollImportLogLines(RequestLogLine param)
        {
            dynamic response = new ExpandoObject();

            try
            {
                var logFilePath = param.LogFilePath;
                var lineCount = param.LineCount;

                response.status = "200";
                response.result = _importService.GetLinesFromImportProtocol(lineCount, logFilePath);
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't fetch import log lines, error in logs";
                _logger.LogError(ex, "Error fetching new import log lines:");
            }

            return Json(response);
        }
    }
}
