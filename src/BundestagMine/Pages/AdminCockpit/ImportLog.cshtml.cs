using BundestagMine.Services;
using BundestagMine.ViewModels.Import;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BundestagMine.Pages.AdminCockpit
{
    public class ImportLogModel : PageModel
    {
        private readonly ImportService _importService;

        public ImportLogModel(ImportService importService)
        {
            _importService = importService;
        }

        public ImportLogViewModel ImportLog { get; set; }

        public IActionResult OnGet(string filename)
        {
            //if (!User.Identity.IsAuthenticated) return RedirectToPage("Index");

            ImportLog = _importService.BuildImportLogViewModel(filename, true);

            return Page();
        }
    }
}
