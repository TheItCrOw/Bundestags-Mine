using BundestagMine.Models.Database.MongoDB;
using BundestagMine.SqlDatabase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace BundestagMine.Pages.PublicViews
{
    public class SpeechFulltextAnalysisWindowModel : PageModel
    {
        private readonly BundestagMineDbContext _db;

        [BindProperty]
        public Speech Speech { get; set; }

        public SpeechFulltextAnalysisWindowModel(BundestagMineDbContext db)
        {
            _db = db;
        }

        public async Task OnGet(string speechId)
        {
            if (!string.IsNullOrEmpty(speechId) && Guid.TryParse(speechId, out var id))
                Speech = await _db.Speeches.FindAsync(id);
        }
    }
}
