using BundestagMine.Logic.Services;
using BundestagMine.Services;
using BundestagMine.SqlDatabase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Controllers
{
    [Route("api/ParliamentPanoramaController")]
    [ApiController]
    public class ParliamentPanoramaController : Controller
    {
        private readonly ViewRenderService _viewRenderService;
        private readonly CategoryService _categoryService;
        private readonly ILogger<ParliamentPanoramaController> _logger;
        private readonly BundestagMineDbContext _db;

        public ParliamentPanoramaController(BundestagMineDbContext db, 
            ILogger<ParliamentPanoramaController> logger,
            CategoryService categoryService,
            ViewRenderService viewRenderService)
        {
            _viewRenderService = viewRenderService;
            _categoryService = categoryService;
            _logger = logger;
            _db = db;
        }

        [HttpGet("/api/ParliamentPanoramaController/GetCategoryLineChartData/{categoryNameAsBase64}")]
        public IActionResult GetCategoryLineChartData(string categoryNameAsBase64)
        {
            dynamic response = new ExpandoObject();

            try
            {
                var categoryName = Encoding.UTF8.GetString(Convert.FromBase64String(categoryNameAsBase64));
                response.result = _categoryService.GetAmountOfSpeechesByYearOfCategory(categoryName);
                response.status = "200";
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't build the category linechart data.";
                _logger.LogError(ex, "Error creating category linechart data:");
            }

            return Json(response);
        }

        [HttpGet("/api/ParliamentPanoramaController/GetCategoriesPanoramaView/")]
        public async Task<IActionResult> GetCategoriesPanoramaView()
        {
            dynamic response = new ExpandoObject();

            try
            {
                var categories = _categoryService.GetDistinctCategoriesAndSubCategories();
                response.result = await _viewRenderService.RenderToStringAsync("ParliamentPanorama/_CategoriesPanoramaView", categories);
                response.status = "200";
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't build the categories panorama.";
                _logger.LogError(ex, "Error creating the categories panorama:");
            }

            return Json(response);
        }
    }
}
