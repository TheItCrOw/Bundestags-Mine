using BundestagMine.Logic.HelperModels;
using BundestagMine.Logic.ViewModels.ParliamentPanorama;
using BundestagMine.SqlDatabase;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Logic.Services
{
    public class CategoryService
    {
        private readonly BundestagMineDbContext _db;
        private readonly ILogger<CategoryService> _logger;
        private readonly string[] _blacklistedCategories;

        public CategoryService(BundestagMineDbContext db,
            ILogger<CategoryService> logger)
        {
            _db = db;
            _logger = logger;
            _blacklistedCategories = ConfigManager.GetBlacklistedCategories();
        }

        /// <summary>
        /// Gets the amount of speeches by each year used for charts.
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public List<DefaultChartModel> GetAmountOfSpeechesByYearOfCategory(
            string categoryName)
        {
            return _db.Categories.Where(c => c.Name == categoryName || c.SubCategory == categoryName)
                // Only interested here in speeches which arent just "Ich nehme die Wahl an!" or something.
                .SelectMany(c => _db.Speeches.Where(s => s.Id == c.NLPSpeechId && s.Text.Length > 150))
                .Select(s => new
                {
                    Year = _db.Protocols.FirstOrDefault(p => p.LegislaturePeriod == s.LegislaturePeriod && p.Number == s.ProtocolNumber).Date.Year,
                    Speech = s.Id
                })
                .AsEnumerable()
                .GroupBy(o => o.Year)
                .OrderBy(o => o.Key)
                .Select(g => new DefaultChartModel()
                {
                    Label = g.Key.ToString(),
                    Value = g.Count()
                })
                .ToList();
        }

        /// <summary>
        /// Gets a list of all distinct categories with their subcategories as CategoryViewModels 
        /// </summary>
        /// <returns></returns>
        public List<CategoryViewModel> GetDistinctCategoriesAndSubCategories()
        {
            return _db.Categories
                .GroupBy(c => new { c.Name })
                .Select(g => new CategoryViewModel()
                {
                    Name = g.Key.Name,
                    SubCategories = g.Select(s => s.SubCategory).Distinct().ToList() ?? new List<string>()
                })
                .AsEnumerable()
                .GroupBy(vm => vm.Name)
                .Select(g => new CategoryViewModel()
                {
                    Name = g.Key,
                    SubCategories = g.SelectMany(vm => vm.SubCategories).Distinct().ToList()
                })
                .Where(c => !_blacklistedCategories.Contains(c.Name))
                .ToList();
        }
    }
}
