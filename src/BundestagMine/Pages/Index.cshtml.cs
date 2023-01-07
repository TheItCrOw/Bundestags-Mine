using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BundestagMine.Logic.Services;
using BundestagMine.Logic.ViewModels.DailyPaper;
using BundestagMine.Models;
using BundestagMine.SqlDatabase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BundestagMine.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DailyPaperService _dailyPaperService;
        private readonly GraphService _graphService;
        private readonly BundestagMineDbContext _db;
        private readonly ILogger<IndexModel> _logger;

        public DailyPaperViewModel DailyPaper { get; set; }

        public IndexModel(ILogger<IndexModel> logger, 
            BundestagMineDbContext db,
            GraphService graphService,
            DailyPaperService dailyPaperService)
        {
            _dailyPaperService = dailyPaperService;
            _graphService = graphService;
            _db = db;
            _logger = logger;
        }

        public void OnGet()
        {
            // Just for development
            DailyPaper = _dailyPaperService.BuildDailyPaperViewModel(76, 20);
        }
    }
}
