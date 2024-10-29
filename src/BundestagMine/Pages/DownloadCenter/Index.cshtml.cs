using BundestagMine.Logic.Services;
using BundestagMine.Logic.ViewModels.DownloadCenter;
using BundestagMine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BundestagMine.Pages.DownloadCenter
{
    public class IndexModel : PageModel
    {
        private readonly DownloadCenterService _downloadCenterService;

        [BindProperty]
        public DownloadableZipFileViewModel ZipFileViewModel  { get; set; }

        public IndexModel(DownloadCenterService downloadCenterService)
        {
            _downloadCenterService = downloadCenterService;
        }

        public void OnGet(string filename)
        {
            // Fetch the downloadable file by its name and show it.
            ZipFileViewModel = _downloadCenterService.GetZipFileAsViewModel(filename);
        }
    }
}
