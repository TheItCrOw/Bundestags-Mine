using BundestagMine.Logic.Services;
using BundestagMine.RequestModels;
using BundestagMine.Services;
using BundestagMine.SqlDatabase;
using BundestagMine.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.Controllers
{
    [Route("api/DownloadCenterController")]
    [ApiController]
    public class DownloadCenterController : Controller
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly DownloadCenterService _downloadCenterService;
        private readonly GlobalSearchService _globalSearchService;
        private readonly MetadataService _metadataService;
        private readonly BundestagMineDbContext _db;
        private readonly ILogger<DownloadCenterController> _logger;

        public DownloadCenterController(ILogger<DownloadCenterController> logger,
            GlobalSearchService globalSearchService,
            DownloadCenterService downloadCenterService,
            MetadataService metadataService,
            BundestagMineDbContext db,
            IServiceScopeFactory serviceScopeFactory)
        {
            _metadataService = metadataService;
            _db = db;
            _serviceScopeFactory = serviceScopeFactory;
            _downloadCenterService = downloadCenterService;
            _globalSearchService = globalSearchService;
            _logger = logger;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("/api/DownloadCenterController/DownloadDocumentation/")]
        public IActionResult DownloadDocumentation()
        {
            dynamic response = new ExpandoObject();

            try
            {
                response.status = "200";
                var filepath = "~/files/Download_Center_Doku.pdf";
                Response.Headers.Add("Content-Disposition", "inline; filename=Download_Center_Doku.pdf");
                return File(filepath, "application/pdf");
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't download the zip file, error in logs";
                _logger.LogError(ex, "Error download the data set:");
            }

            return Json(response);
        }

        /// <summary>
        /// Returns a single protocol as a json file as it were in the DownloadCenter. 
        /// This can't be tried in the swagger UI, as the response is too large (might take a few seconds as well). 
        /// Try an example call in a new browser tab like <br/>: https://bundestag-mine/api/DownloadCenterController/DownloadProtocol/20%2C%20175
        /// </summary>
        /// <param name="param">legislature_periode + ',' + protocol_number</param>
        /// <returns></returns>
        [HttpGet("/api/DownloadCenterController/DownloadProtocol/{param}")]
        public IActionResult DownloadProtocol(string param)
        {
            dynamic response = new ExpandoObject();

            try
            {
                var splited = param.Split(",");
                var period = int.Parse(splited[0]);
                var protocolNumber = int.Parse(splited[1]);
                var protocol = _db.Protocols
                    .FirstOrDefault(p => p.LegislaturePeriod == period && p.Number == protocolNumber);

                response.status = "200";
                response.result = _downloadCenterService.CreateDownloadProtocolFromProtocol(
                    protocol, null, null, null, true);
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't download the protocol, error in logs";
                _logger.LogError(ex, "Error download the data set:");
            }

            return Json(response);
        }

        [HttpGet("/api/DownloadCenterController/DownloadDataset/{fileName}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public FileResult DownloadDataset(string fileName)
        {
            dynamic response = new ExpandoObject();

            try
            {
                response.status = "200";
                var path = ConfigManager.GetDownloadCenterFinishedZippedDataSetsDirectory() + $"{fileName}";
                var fileInfo = new FileInfo(path);
                this.Response.ContentLength = fileInfo.Length;
                var zipBytes = System.IO.File.ReadAllBytes(path);

                return File(zipBytes, "application/octet-stream", "export.zip");
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't download the zip file, error in logs";
                _logger.LogError(ex, "Error download the data set:");
            }

            return Json(response);
        }

        [HttpPost("/api/DownloadCenterController/GenerateDatasetByFilter/")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult GenerateDatasetByFilter(FilterDownloadRequest filterDownloadRequest)
        {
            dynamic response = new ExpandoObject();

            try
            {
                if (string.IsNullOrEmpty(filterDownloadRequest.Email))
                {
                    response.status = "400";
                    response.message = "Sie müssen eine Email-Adresse angeben, um den Download-Link zu erhalten.";
                    return Json(response);
                }

                _logger.LogInformation("New dataset generating arrived with the filter: " + filterDownloadRequest.ToString());
                var exportId = Guid.NewGuid();
                var downloadUrl = Request.Scheme + "://" + Request.Host + "/DownloadCenter" + $"?filename=Export_{exportId}.zip";

                Task.Run(() =>
                {
                    try
                    {
                        // We need to create scopes because it runs async.
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var service = scope.ServiceProvider.GetService<DownloadCenterService>();

                            // Generate the dataset, which at the end should be a directory on the harddrive
                            var exportFileName = service.WriteDownloadableProtocolsToDisc(exportId,
                                filterDownloadRequest.From, filterDownloadRequest.To,
                                filterDownloadRequest.Fractions, filterDownloadRequest.Parties, filterDownloadRequest.ExplicitSpeakers);

                            if (string.IsNullOrEmpty(exportFileName))
                            {
                                MailManager.SendMail($"Fehler beim Generieren des Datensatzes",
                                    MailManager.CreateMailHtml("Download Center",
                                    $"Leider ist ein Fehler beim Generieren des Datensatzes mit der Id: {exportFileName} unterlaufen. <br/>" +
                                    $"Probieren Sie diesen neu zu erstellen oder melden Sie den Fehler via Antwort auf diese Mail."
                                    ),
                                    new List<string> { filterDownloadRequest.Email });
                                return;
                            }

                            // Now zip the file
                            var source = ConfigManager.GetDownloadCenterCalculatingDataDirectory() + exportFileName;
                            var target = ConfigManager.GetDownloadCenterFinishedZippedDataSetsDirectory() + exportFileName + ".zip";
                            // Zip it
                            ZipFile.CreateFromDirectory(source, target);
                            // And then delete the unzipped files
                            Directory.Delete(source, true);

                            // Now send a notifcation mail to the user, that the data is ready to download.
                            MailManager.SendMail($"Ihr Datensatz ist fertig!",
                                MailManager.CreateMailWithButtonHtml("Download Center",
                                 "Ihr Datensatz ist fertig und kann heruntergeladen werden!",
                                 "Datensatz",
                                 downloadUrl
                                ),
                                new List<string> { filterDownloadRequest.Email });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while trying to handle a new dataset calculation");
                    }
                });

                // Send a mail with the id of the export and information of the filter
                MailManager.SendMail("Ihre Datensatz-Anfrage",
                    MailManager.CreateMailWithButtonHtml("Download Center",
                        $"Die Kalkulation Ihres Datensatzes wurde angestoßen. <br/>" +
                        $"Ihr Datensatz hat die Id: <b>{exportId}</b>. <br/>" +
                        $"Sie können jederzeit schauen, ob der Datensatz bereit zum Herunterladen ist - " +
                        $"Sie werden jedoch auch per Mail informiert, sobald dies der Fall ist.",
                        "Datensatz",
                        downloadUrl
                    ),
                    new List<string> { filterDownloadRequest.Email });

                response.status = "200";
                response.message = "Die Kalkulation wurde angestoßen und es wird Ihnen per Mail die Bestätigung zugeschickt.";
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't calculate the download data set";
                _logger.LogError(ex, "Error calculating the download data set:");
            }

            return Json(response);
        }

        [HttpPost("/api/DownloadCenterController/CalculateData/")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult CalculateData(FilterDownloadRequest filterDownloadRequest)
        {
            dynamic response = new ExpandoObject();

            try
            {
                response.status = "200";
                var counts = _downloadCenterService.GetProtocolAndSpeechCountOfFilter(
                    filterDownloadRequest.From, filterDownloadRequest.To,
                    filterDownloadRequest.Fractions, filterDownloadRequest.Parties, filterDownloadRequest.ExplicitSpeakers);

                var estimatedFileSizeInMB = Math.Round(counts.Item2 * _downloadCenterService.AverageFileSizeInMBPerNLPSpeech(), 2);
                response.result = new
                {
                    protocols = counts.Item1,
                    speeches = counts.Item2,
                    estimatedMinutes = Math.Round(counts.Item2 * _downloadCenterService.AverageFetchTimeInSecondsPerNLPSpeech() / 60.0f, 0),
                    estimatedFileSizeInMB,
                    estimatedZipFileSizeInMB = Math.Round(estimatedFileSizeInMB / 4.5, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error gathering data:");
                response.status = "400";
                response.message = "Couldn't gather data, error in logs";
            }

            return Json(response);
        }
    }
}
