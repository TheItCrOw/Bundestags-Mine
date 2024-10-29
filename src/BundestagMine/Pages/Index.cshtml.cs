using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BundestagMine.Logic.Services;
using BundestagMine.Logic.ViewModels.DailyPaper;
using BundestagMine.Models;
using BundestagMine.SqlDatabase;
using BundestagMine.Utility;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BundestagMine.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DailyPaperService _dailyPaperService;
        private readonly GraphDataService _graphService;
        private readonly BundestagMineDbContext _db;
        private readonly ILogger<IndexModel> _logger;

        public DailyPaperViewModel DailyPaper { get; set; }

        public List<(int, int, string)> ProtocolNumbers { get; set; }

        public IndexModel(ILogger<IndexModel> logger,
            BundestagMineDbContext db,
            GraphDataService graphService,
            DailyPaperService dailyPaperService)
        {
            _dailyPaperService = dailyPaperService;
            _graphService = graphService;
            _db = db;
            _logger = logger;
        }

        public void OnGet()
        {
            // Get all protocols numbers for the dailypaper
            ProtocolNumbers = _dailyPaperService.GetProtocolsWithDailyPaper()
                .Select(p => new Tuple<int, int, string>(
                    p.LegislaturePeriod, p.Number, DateHelper.DateToGermanDate(p.Date)
                    ).ToValueTuple())
                .OrderByDescending(t => t.Item1)
                .ThenByDescending(t => t.Item2)
                .ToList();
        }
    }
}
