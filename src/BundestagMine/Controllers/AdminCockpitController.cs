using BundestagMine.Services;
using Microsoft.AspNetCore.Mvc;
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
    [ApiController]
    public class AdminCockpitController : Controller
    {
        private readonly ImportService _importService;

        public AdminCockpitController(ImportService importService)
        {
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
            catch (Exception)
            {
                response.status = "400";
                response.message = "Couldn't fetch import log lines, error in logs";
                //TODO: Log
            }

            return Json(response);
        }
    }
}
