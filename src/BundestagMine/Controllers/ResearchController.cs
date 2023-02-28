﻿using BundestagMine.Logic.Services;
using BundestagMine.Models.Database;
using BundestagMine.SqlDatabase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Dynamic;
using System.Linq;

namespace BundestagMine.Controllers
{
    [Route("api/ResearchController")]
    [ApiController]
    public class ResearchController : Controller
    {
        private readonly TextSummarizationService _textSummarizationService;
        private readonly BundestagMineDbContext _db;
        private readonly ILogger<ResearchController> _logger;

        public ResearchController(ILogger<ResearchController> logger, 
            BundestagMineDbContext db,
            TextSummarizationService textSummarizationService)
        {
            _textSummarizationService = textSummarizationService;
            _db = db;
            _logger = logger;
        }


        [HttpPost("/api/ResearchController/GetNLPSpeeches/")]
        public IActionResult GetNLPSpeeches()
        {
            try
            {
                // Using a jquery datatable in frontend.
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                var speeches = _db.NLPSpeeches
                    .Where(s => !string.IsNullOrEmpty(s.EnglishTranslationOfSpeech) && s.ProtocolNumber > 70)
                    .Where(s => string.IsNullOrEmpty(searchValue) || s.Text.ToLower().Contains(searchValue.ToLower()));

                recordsTotal = speeches.Count();
                var data = speeches.Skip(skip)
                    .Take(pageSize)
                    .AsEnumerable()
                    .Select(s =>
                    {
                        var evaluations = _textSummarizationService.GetEvaluationsOfSpeech(s.Id);
                        dynamic obj = new
                        {
                            s.Id,
                            s.Text,
                            s.EnglishTranslationOfSpeech,
                            s.ProtocolNumber,
                            s.ExtractiveSummary,
                            TextRankEval = evaluations.FirstOrDefault(e => e.TextSummarizationMethod == TextSummarizationMethods.TextRank),
                            s.AbstractSummary,
                            BartEval = evaluations.FirstOrDefault(e => e.TextSummarizationMethod == TextSummarizationMethods.BARTSamSum),
                            s.AbstractSummaryPEGASUS,
                            PegasusEval = evaluations.FirstOrDefault(e => e.TextSummarizationMethod == TextSummarizationMethods.PEGASUSSamSum)
                        };
                        return obj;
                    });

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    if (sortColumn == "TR-S.")
                        if (sortColumnDirection == "asc") data = data.OrderBy(s => s.TextRankEval.SummaryScore);
                        else if (sortColumnDirection == "desc") data = data.OrderByDescending(s => s.TextRankEval.SummaryScore);
                    if (sortColumn == "Ü-S.")
                        if (sortColumnDirection == "asc") data = data.OrderBy(s => s.ProtocolNumber);
                        else if (sortColumnDirection == "desc") data = data.OrderByDescending(s => s.ProtocolNumber);
                    if (sortColumn == "P-S.")
                        if (sortColumnDirection == "asc") data = data.OrderBy(s => s.PegasusEval.SummaryScore);
                        else if (sortColumnDirection == "desc") data = data.OrderByDescending(s => s.PegasusEval.SummaryScore);
                    if (sortColumn == "B-S.")
                        if (sortColumnDirection == "asc") data = data.OrderBy(s => s.BartEval.SummaryScore);
                        else if (sortColumnDirection == "desc") data = data.OrderByDescending(s => s.BartEval.SummaryScore);
                }

                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data.ToList() };
                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching new nlp speeches for research center:");
                return null;
            }
        }
    }
}
