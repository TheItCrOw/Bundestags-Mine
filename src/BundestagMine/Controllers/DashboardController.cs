using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using BundestagMine.Extensions;
using BundestagMine.Models.Database.MongoDB;
using BundestagMine.Services;
using BundestagMine.SqlDatabase;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Supremes;
using Newtonsoft.Json;
using BundestagMine.Utility;
using BundestagMine.RequestModels;
using BundestagMine.ViewModels;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.Extensions.Logging;

namespace BundestagMine.Controllers
{
    [Route("api/DashboardController")]
    [ApiController]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly TopicAnalysisService _topicAnalysisService;
        private readonly BundestagScraperService _bundestagScraperService;
        private readonly GraphService _graphService;
        private readonly MetadataService _metadataService;
        private readonly ViewRenderService _viewRenderService;
        private readonly AnnotationService _annotationService;
        private readonly BundestagMineDbContext _db;

        public DashboardController(BundestagMineDbContext db, AnnotationService tokenService,
            ViewRenderService viewRenderService,
            MetadataService metadataService,
            GraphService graphService,
            BundestagScraperService bundestagScraperService,
            TopicAnalysisService topicAnalysisService,
            ILogger<DashboardController> logger)
        {
            _logger = logger;
            _topicAnalysisService = topicAnalysisService;
            _bundestagScraperService = bundestagScraperService;
            _graphService = graphService;
            _metadataService = metadataService;
            _viewRenderService = viewRenderService;
            _annotationService = tokenService;
            _db = db;
        }

        [HttpGet("/api/DashboardController/GetProtocols")]
        public IActionResult GetProtocols()
        {
            dynamic response = new ExpandoObject();

            try
            {
                response.status = "200";
                response.result = _db.Protocols
                    .Select(p => new
                    {
                        p.Date,
                        p.AgendaItemsCount,
                        p.Id,
                        p.LegislaturePeriod,
                        p.MongoId,
                        p.Number,
                        p.Title,
                        PollsAmount = _db.Polls.Where(poll => poll.LegislaturePeriod == p.LegislaturePeriod && poll.ProtocolNumber == p.Number).Count()
                    })
                    .OrderByDescending(p => p.LegislaturePeriod).ThenByDescending(p => p.Number).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching protocols:");
                response.status = "400";
                response.message = "Couldn't fetch protocols, error in logs";
            }

            return Json(response);
        }

        [HttpGet("/api/DashboardController/GetParties")]
        public IActionResult GetParties()
        {
            dynamic response = new ExpandoObject();

            try
            {
                response.status = "200";
                var parties = new List<dynamic>();
                foreach (var deputy in _db.Deputies.ToList())
                {
                    if (!string.IsNullOrEmpty(deputy.Party) && !parties.Any(p => p.id == deputy.Party))
                    {
                        dynamic party = new ExpandoObject();
                        party.id = deputy.Party;
                        parties.Add(party);
                    }
                }
                response.result = parties;
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't fetch parties, error in logs";
                _logger.LogError(ex, "Error fetching parties:");
            }

            return Json(response);
        }

        [HttpGet("/api/DashboardController/GetFractions")]
        public IActionResult GetFractions()
        {
            dynamic response = new ExpandoObject();

            try
            {
                response.status = "200";
                response.result = _metadataService.GetFractions();

            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't fetch fractions, error in logs";
                _logger.LogError(ex, "Error fetching fractions:");
            }

            return Json(response);
        }

        [HttpGet("/api/DashboardController/GetAgendaItemsOfProtocol/{protocolIdAsString}")]
        public IActionResult GetAgendaItemsOfProtocol(string protocolIdAsString)
        {
            dynamic response = new ExpandoObject();

            try
            {
                var id = Guid.Parse(protocolIdAsString);
                response.status = "200";
                response.result = _db.AgendaItems.Where(a => a.ProtocolId == id).OrderBy(a => a.Order).ToList();
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't fetch agendaitems, error in logs";
                _logger.LogError(ex, "Error fetching agenda items of protocol:");
            }

            return Json(response);
        }

        [HttpGet("/api/DashboardController/GetPollsOfProtocol/{param}")]
        public IActionResult GetPollsOfProtocol(string param)
        {
            dynamic response = new ExpandoObject();

            try
            {
                var splited = param.Split(",");
                var legislaturePeriod = int.Parse(splited[0]);
                var protocolNumber = int.Parse(splited[1]);
                response.status = "200";
                response.result = _db.Polls.Where(p => p.LegislaturePeriod == legislaturePeriod && p.ProtocolNumber == protocolNumber)
                    .ToList();
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't fetch polls for protocols, error in logs";
                _logger.LogError(ex, "Error fetching polls of protocol:");
            }

            return Json(response);
        }

        [HttpGet("/api/DashboardController/GetBundestagUrlOfPoll/{pollIdAsString}")]
        public async Task<IActionResult> GetBundestagUrlOfPoll(string pollIdAsString)
        {
            dynamic response = new ExpandoObject();

            try
            {
                var pollId = Guid.Parse(pollIdAsString);
                var poll = await _db.Polls.FindAsync(pollId);
                response.status = "200";
                response.result = _bundestagScraperService.GetBundestagUrlOfPoll(poll);
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = $"Couldn't fetch url for poll, error in logs";
                _logger.LogError(ex, "Error fetching bundestag url for poll:");
            }

            return Json(response);
        }

        [HttpGet("/api/DashboardController/GetSpeakerById/{speakerId}")]
        public IActionResult GetSpeakerById(string speakerId)
        {
            dynamic response = new ExpandoObject();

            try
            {
                response.status = "200";
                response.result = _db.Deputies.FirstOrDefault(d => d.SpeakerId == speakerId);
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't fetch fractions, error in logs";
                _logger.LogError(ex, "Error fetching speaker by id:");
            }

            return Json(response);
        }

        /// <summary>
        /// Param[0] = limit, Param[1] = from, Param[2] = to, Param[3] = fraction, Param[4] = party,
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpGet("/api/DashboardController/GetSpeaker/{param}")]
        public IActionResult GetSpeaker(string param)
        {
            dynamic response = new ExpandoObject();

            try
            {
                var splited = param.ToCleanRequestString().Split(',');
                var limit = splited[0] == "" ? 20 : int.Parse(splited[0]);
                var fraction = splited[3];
                var party = splited[4];
                // If we got these parameters, we want to return reduced speakers for the chart.
                // TODO: This shouldnt be one endpoint in the controller...
                if (DateTime.TryParse(splited[1], out var from) && DateTime.TryParse(splited[2], out var to))
                {
                    var speakerIdToCount = _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                        .SelectMany(p => _db.Speeches
                            .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod))
                        .SelectMany(s => _db.Deputies.Where(d => d.SpeakerId == s.SpeakerId && (string.IsNullOrEmpty(fraction) || d.Fraction == fraction)
                                && (string.IsNullOrEmpty(party) || d.Party == party)))
                        .GroupBy(s => s.SpeakerId)
                        .Select(t => new { Deputy = _db.Deputies.FirstOrDefault(d => d.SpeakerId == t.Key), Count = t.Count() })
                        .OrderByDescending(kv => kv.Count)
                        .Take(limit)
                        .ToList();

                    var dynamicDeputies = new List<dynamic>();
                    foreach (var pair in speakerIdToCount)
                    {
                        dynamic dynamicDeputy = new ExpandoObject();
                        dynamicDeputy.id = pair.Deputy.SpeakerId;
                        dynamicDeputy.firstName = pair.Deputy.FirstName;
                        dynamicDeputy.lastName = pair.Deputy.LastName;
                        dynamicDeputy.speakerCount = pair.Count;

                        dynamicDeputies.Add(dynamicDeputy);
                    }

                    response.result = dynamicDeputies;
                }
                else
                {
                    response.result = _db.Deputies.ToList();
                }

                response.status = "200";
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't fetch speaker, error in logs";
                _logger.LogError(ex, "Error fetching speakers:");
            }

            return Json(response);
        }


        [HttpGet("/api/DashboardController/GetSpeechesOfAgendaItem/{param}")]
        public IActionResult GetSpeechesOfAgendaItem(string param)
        {
            dynamic response = new ExpandoObject();

            try
            {
                var splited = param.ToCleanRequestString().Split(",");
                var period = int.Parse(splited[0]);
                var protocol = int.Parse(splited[1]);
                var number = int.Parse(splited[2]);
                response.status = "200";
                response.result = _db.Speeches
                    .Where(s => s.LegislaturePeriod == period && s.ProtocolNumber == protocol && s.AgendaItemNumber == number)
                    .ToList();
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't fetch agenda items, error in logs";
                _logger.LogError(ex, "Error fetching speeches of agenda item:");
            }

            return Json(response);
        }

        [HttpGet("/api/DashboardController/GetNLPSpeechById/{idAsString}")]
        public async Task<IActionResult> GetNLPSpeechById(string idAsString)
        {
            dynamic response = new ExpandoObject();

            try
            {
                var id = Guid.Parse(idAsString);
                response.status = "200";

                var speech = await _db.NLPSpeeches.FindAsync(id);
                speech.Tokens = _db.Token.Where(t => t.NLPSpeechId == id && t.ShoutId == Guid.Empty).OrderBy(t => t.Begin).ToList();
                speech.NamedEntities = _db.NamedEntity.Where(t => t.NLPSpeechId == id && t.ShoutId == Guid.Empty).OrderBy(t => t.Begin).ToList();
                speech.Sentiments = _db.Sentiment.Where(t => t.NLPSpeechId == id && t.ShoutId == Guid.Empty).OrderBy(t => t.Begin).ToList();
                speech.Segments = _db.SpeechSegment.Include(s => s.Shouts).Where(s => s.SpeechId == speech.Id).ToList();
                //speech.CategoryCoveredTags = _db.CategoryCoveredTagged.Where(c => c.NLPSpeechId == speech.Id)
                //    .OrderByDescending(t => t.Score).ToList();

                // We want the top X named entities which build the topic of this speech.
                var topics = speech.NamedEntities
                    .GroupBy(ne => ne.LemmaValue)
                    .OrderByDescending(g => g.Count())
                    .Select(ne => new
                    {
                        Value = ne.Key,
                        Count = ne.Count()
                    })
                    .Take(5)
                    .ToList();

                response.result = new
                {
                    speech,
                    topics,
                    agendaItem = _metadataService.GetAgendaItemOfSpeech(speech)
                };
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't fetch nlp speech, error in logs";
                _logger.LogError(ex, "Error fetching nlp speech:");
            }

            return Json(response);
        }

        [HttpGet("/api/DashboardController/GetCommentNetworkData/")]
        public IActionResult GetCommentNetworkData()
        {
            dynamic response = new ExpandoObject();

            try
            {
                response.status = "200";
                var data = _db.NetworkDatas.First();
                data.Links = _db.CommentNetworkLink.ToList();
                data.Nodes = _db.CommentNetworkNode.ToList();
                response.result = data;
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't fetch netweork data speech, error in logs";
                _logger.LogError(ex, "Error fetching comment network data:");
            }

            return Json(response);
        }

        [HttpGet("/api/DashboardController/GetTopicMapChartData/{year}")]
        public IActionResult GetTopicMapChartData(string year)
        {
            dynamic response = new ExpandoObject();

            try
            {
                response.status = "200";
                var jsonString = System.IO.File.ReadAllText($"{ConfigManager.GetDataDirectoryPath()}topicMap_{year}.json");
                response.result = JsonConvert.DeserializeObject(jsonString, typeof(TopicMapGraphObject));
            }
            catch (Exception ex)
            {
                response.status = "400";
                _logger.LogError(ex, "Error fetching topic map chart data:");
            }

            return Json(response);
        }

        [HttpGet("/api/DashboardController/GetTopicBarRaceChartData/")]
        public IActionResult GetTopicBarRaceChartData()
        {
            dynamic response = new ExpandoObject();

            try
            {
                response.status = "200";
                var jsonString = System.IO.File.ReadAllText($"{ConfigManager.GetDataDirectoryPath()}topicBarRaceData.json");
                response.result = JsonConvert.DeserializeObject(jsonString, typeof(List<TopicBarRaceGraphObject>));
            }
            catch (Exception ex)
            {
                response.status = "400";
                _logger.LogError(ex, "Error fetching topic bar race data:");
            }

            return Json(response);
        }

        [HttpGet("/api/DashboardController/GetTokens/{param}")]
        public IActionResult GetTokens(string param)
        {
            dynamic response = new ExpandoObject();

            try
            {
                var splited = param.ToCleanRequestString().Split(',');
                var limit = int.Parse(splited[0]);
                var from = DateTime.Parse(splited[1]);
                var to = DateTime.Parse(splited[2]);
                var fraction = splited[3];
                var party = splited[4];
                var speakerId = splited[5];

                response.result = _annotationService.GetTokensForGraphs(limit, from, to, fraction, party, speakerId);
                response.status = "200";
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't fetch nlp tokens, error in logs";
                _logger.LogError(ex, "Error fetching tokens:");
            }

            return Json(response);
        }

        [HttpGet("/api/DashboardController/GetPOS/{param}")]
        public IActionResult GetPOS(string param)
        {
            dynamic response = new ExpandoObject();

            try
            {
                var splited = param.ToCleanRequestString().Split(',');
                var limit = int.Parse(splited[0]);
                var from = DateTime.Parse(splited[1]);
                var to = DateTime.Parse(splited[2]);
                var fraction = splited[3];
                var party = splited[4];
                var speakerId = splited[5];

                response.result = _annotationService.GetPOSForGraphs(limit, from, to, fraction, party, speakerId);
                response.status = "200";
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't fetch nlp pos, error in logs";
                _logger.LogError(ex, "Error fetching POS:");
            }

            return Json(response);
        }

        [HttpGet("/api/DashboardController/GetSentiments/{param}")]
        public IActionResult GetSentiments(string param)
        {
            dynamic response = new ExpandoObject();

            try
            {
                var splited = param.ToCleanRequestString().Split(',');
                var from = DateTime.Parse(splited[0]);
                var to = DateTime.Parse(splited[1]);
                var fraction = splited[2];
                var party = splited[3];
                var speakerId = splited[4];

                response.result = _annotationService.GetSentimentsForGraphs(from, to, fraction, party, speakerId);
                response.status = "200";
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't fetch nlp sentiments, error in logs";
                _logger.LogError(ex, "Error fetching sentiments:");
            }

            return Json(response);
        }

        [HttpGet("/api/DashboardController/GetNamedEntitites/{param}")]
        public IActionResult GetNamedEntitites(string param)
        {
            dynamic response = new ExpandoObject();

            try
            {
                var splited = param.ToCleanRequestString().Split(',');
                var limit = int.Parse(splited[0]);
                var from = DateTime.Parse(splited[1]);
                var to = DateTime.Parse(splited[2]);
                var fraction = splited[3];
                var party = splited[4];
                var speakerId = splited[5];

                response.result = _annotationService.GetNamedEntitiesForGraph(limit, from, to, fraction, party, speakerId);
                response.status = "200";
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't fetch nlp NE, error in logs";
                _logger.LogError(ex, "Error fetching named entities:");
            }

            return Json(response);
        }

        [HttpGet("/api/DashboardController/SearchNamedEntities/{searchString}")]
        public IActionResult SearchNamedEntities(string searchString)
        {
            dynamic response = new ExpandoObject();

            try
            {
                searchString = searchString.ToCleanRequestString();
                response.status = "200";
                response.result = _db.NamedEntity
                    .Where(ne => ne.LemmaValue.ToLower().Trim().Contains(searchString.ToLower().Trim()))
                    .GroupBy(ne => ne.LemmaValue)
                    .OrderByDescending(kv => kv.Count())
                    .Take(50)
                    .Select(kv => new { Element = kv.Key, Count = kv.Count() })
                    .ToList();
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = $"Couldn't search named entities, error in logs";
                _logger.LogError(ex, "Error searching named entities:");
            }

            return Json(response);
        }

        [HttpGet("/api/DashboardController/GetSpeakerSentimentsAboutNamedEntity/{param}")]
        public IActionResult GetSpeakerSentimentsAboutNamedEntity(string param)
        {
            dynamic response = new ExpandoObject();

            try
            {
                var splited = param.ToCleanRequestString().Split(',');
                var from = DateTime.Parse(splited[0]);
                var to = DateTime.Parse(splited[1]);
                var speakerId = splited[2];
                var namedEntity = splited[3];

                response.result = _annotationService.GetNamedEntityWithCorrespondingSentiment(namedEntity, from, to, "", "", speakerId);
                response.status = "200";
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't fetch nlp sentiments for speaker, error in logs";
                _logger.LogError(ex, "Error fetching speaker sentiment about ne:");
            }

            return Json(response);
        }

        [HttpGet("/api/DashboardController/SearchSpeeches/{searchTerm}")]
        public IActionResult SearchSpeeches(string searchTerm)
        {
            dynamic response = new ExpandoObject();

            try
            {
                searchTerm = searchTerm.ToCleanRequestString();

                response.result = _db.Speeches
                    .Where(s => s.Text.ToLower().Contains(searchTerm.ToLower()))
                    .Take(500)
                    .Select(s => new
                    {
                        Speech = s,
                        AgendaItem = _metadataService.GetAgendaItemOfSpeech(s)
                    })
                    .ToList();
                response.status = "200";
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't fetch nlp speeches, error in logs";
                _logger.LogError(ex, "Error searching speeches:");
            }

            return Json(response);
        }

        [HttpGet("/api/DashboardController/GetHomescreenData")]
        public IActionResult GetHomescreenData()
        {
            dynamic response = new ExpandoObject();

            try
            {
                response.result = new ExpandoObject();
                response.result.protocols = _db.Protocols.Count();
                response.result.speeches = _db.Speeches.Count();
                response.result.deputies = _db.Deputies.Count();
                response.result.tokens = _db.Token.Count();
                response.status = "200";
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't fetch homescreen data, error in logs";
                _logger.LogError(ex, "Error fetching homescreen data:");
            }

            return Json(response);
        }

        [HttpGet("/api/DashboardController/GetDeputyPortrait/{speakerId}")]
        public async Task<IActionResult> GetDeputyPortrait(string speakerId)
        {
            dynamic response = new ExpandoObject();

            try
            {
                var deputy = await _db.Deputies.FirstOrDefaultAsync(d => d.SpeakerId == speakerId);
                if (deputy == null)
                {
                    response.status = 400;
                    response.message = "Deputy not found";
                    return response;
                }
                response.result = _bundestagScraperService.GetDeputyPortraitFromImageDatabase(deputy);
                response.status = "200";
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't fetch image, error in logs";
                _logger.LogError(ex, "Error fetching deputy portrait:");
            }

            return Json(response);
        }

        [HttpPost("/api/DashboardController/PostNewTopicAnalysis/")]
        public async Task<IActionResult> PostNewTopicAnalysis(TopicAnalysisConfigurationRequest configurationRequest)
        {
            dynamic response = new ExpandoObject();

            try
            {
                string BuildFullName(Deputy deputy) => deputy == null ? "Unbekannt" : deputy.FirstName + " " + deputy.LastName;

                var reportVm = new TopicAnalysisReportViewModel()
                {
                    Name = configurationRequest.Name,
                    ReportId = configurationRequest.Id,
                    From = configurationRequest.From,
                    To = configurationRequest.To,
                    Topic = configurationRequest.TopicLemmaValue,
                    Entities = configurationRequest.Fractions.Select(f => (f, f, "Fraktion"))
                            .Concat(configurationRequest.Parties.Select(f => (f, f, "Partei")))
                            .Concat(configurationRequest.SpeakerIds
                                .Select(f => (f, BuildFullName(_db.Deputies.FirstOrDefault(d => d.Id.ToString() == f)), "Redner(in)")))
                            .ToList()
                };

                response.status = "200";
                response.result = await _viewRenderService.RenderToStringAsync("TopicAnalysis/_TopicAnalysisReportView", reportVm);
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't post topic analysis report, error in logs";
                _logger.LogError(ex, "Error posting new topic analysis:");
            }

            return Json(response);
        }

        [HttpPost("/api/DashboardController/BuildReportPage/")]
        public async Task<IActionResult> BuildReportPage(ReportPageRequest reportPageRequest)
        {
            dynamic response = new ExpandoObject();

            try
            {
                // For the request we had to escape the german unmlate clientsided. Conver them back.
                reportPageRequest.Topic = DateHelper.ConvertGermanUmlauteBack(reportPageRequest.Topic);
                reportPageRequest.Fraction = DateHelper.ConvertGermanUmlauteBack(reportPageRequest.Fraction);
                reportPageRequest.Party = DateHelper.ConvertGermanUmlauteBack(reportPageRequest.Party);

                // IMPORTANT: reportPageRequest.SpeakerId is actually DeputyId
                var pageVm = new ReportPageViewModel()
                {
                    Id = Guid.NewGuid(),
                    PageNumber = reportPageRequest.PageNumber,
                    ReportId = reportPageRequest.ReportId,
                    From = DateTime.Parse(reportPageRequest.From),
                    To = DateTime.Parse(reportPageRequest.To),
                    Topic = reportPageRequest.Topic
                };

                // Build the sentiment chart as an object.
                var graphData = _annotationService.GetNamedEntityWithCorrespondingSentiment(
                    reportPageRequest.Topic, pageVm.From, pageVm.To,
                    reportPageRequest.Fraction, reportPageRequest.Party,
                    _db.Deputies.FirstOrDefault(d => d.Id.ToString() == reportPageRequest.SpeakerId)?.SpeakerId);
                pageVm.SentimentGraphData = graphData;

                // Build the topic speeches
                pageVm.TopicSpeeches = _annotationService.GetSpeechesOfNamedEnitity(
                    20, reportPageRequest.Fraction, reportPageRequest.Party, reportPageRequest.SpeakerId,
                    pageVm.From, pageVm.To, pageVm.Topic);

                // Build the topic comments from the chosen entity
                pageVm.TopicCommentsFromEntity = _annotationService.GetCommentsAboutTopic(
                    20, reportPageRequest.Fraction, reportPageRequest.Party, reportPageRequest.SpeakerId,
                    pageVm.From, pageVm.To, pageVm.Topic);

                // Build the topic compared to other topics graph
                pageVm.TopicToOtherTopicsCompareGraphData = _annotationService.GetNamedEntityComparedToOtherNamedEntities(
                    20, reportPageRequest.Fraction, reportPageRequest.Party, reportPageRequest.SpeakerId,
                    pageVm.From, pageVm.To, pageVm.Topic);

                // Build the polls
                pageVm.TopicPolls = _topicAnalysisService.GetPollsOfTopic(
                    20, pageVm.Topic, pageVm.From, pageVm.To, reportPageRequest.Fraction,
                    reportPageRequest.Party, reportPageRequest.SpeakerId);

                // speaker page
                if (!string.IsNullOrEmpty(reportPageRequest.SpeakerId))
                {
                    var deputy = _db.Deputies.FirstOrDefault(d => d.Id.ToString() == reportPageRequest.SpeakerId);
                    if (deputy != null)
                    {
                        pageVm.StatisticsViewModel = _topicAnalysisService.BuildDeputyViewModel(
                            deputy, pageVm.From, pageVm.To, pageVm.Topic);
                    }
                }
                // Fraction Page
                else if (!string.IsNullOrEmpty(reportPageRequest.Fraction))
                {
                    pageVm.StatisticsViewModel = _topicAnalysisService.BuildFractionViewModel(
                            reportPageRequest.Fraction, pageVm.From, pageVm.To, pageVm.Topic);
                }
                // Party Page
                else if (!string.IsNullOrEmpty(reportPageRequest.Party))
                {
                    pageVm.StatisticsViewModel = _topicAnalysisService.BuildPartyViewModel(
                        reportPageRequest.Party, pageVm.From, pageVm.To, pageVm.Topic);
                }

                response.status = "200";
                response.result = await _viewRenderService.RenderToStringAsync("TopicAnalysis/_ReportPage", pageVm);
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't build page, error in logs";
                _logger.LogError(ex, "Error building report page:");
            }

            return Json(response);
        }
    }
}