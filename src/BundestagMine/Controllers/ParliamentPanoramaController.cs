using BundestagMine.Logic.Services;
using BundestagMine.Logic.ViewModels;
using BundestagMine.Services;
using BundestagMine.SqlDatabase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

        [HttpGet("/api/ParliamentPanoramaController/GetSpeechesViewForCategory/{param}")]
        public async Task<IActionResult> GetSpeechesViewForCategory(string param)
        {
            dynamic response = new ExpandoObject();

            try
            {
                var splited = param.Split(',');
                var categoryNameAsBase64 = splited[0];
                var subcategoryNameAsBase64 = splited[1];
                var skip = int.Parse(splited[2]);
                var take = 25; // Lets hardcode for now. Not sure whether I will give the user an option to define this.

                var categoryName = Encoding.UTF8.GetString(Convert.FromBase64String(categoryNameAsBase64));
                var subcategoryName = Encoding.UTF8.GetString(Convert.FromBase64String(subcategoryNameAsBase64));

                var speechesList = _categoryService.GetSpeechViewModelsOfCategory(categoryName, subcategoryName, skip, take);
                response.result = await _viewRenderService.RenderToStringAsync("_SpeechViewModelListView", speechesList);
                response.status = "200";
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't build the speeches view for category.";
                _logger.LogError(ex, "Error creating speeches view for category:");
            }

            return Json(response);
        }

        [HttpGet("/api/ParliamentPanoramaController/GetCategoryLineChartData/{param}")]
        public IActionResult GetCategoryLineChartData(string param)
        {
            dynamic response = new ExpandoObject();

            try
            {
                var splited = param.Split(',');
                var categoryNameAsBase64 = splited[0];
                var subcategoryNameAsBase64 = splited[1];

                var categoryName = Encoding.UTF8.GetString(Convert.FromBase64String(categoryNameAsBase64));
                var subcategoryName = Encoding.UTF8.GetString(Convert.FromBase64String(subcategoryNameAsBase64));
                response.result = _categoryService.GetAmountOfSpeechesByYearOfCategory(categoryName, subcategoryName);
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
