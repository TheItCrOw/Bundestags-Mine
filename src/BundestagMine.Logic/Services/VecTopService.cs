using BundestagMine.Models.Database;
using BundestagMine.Models.Database.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.Utility;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Logic.Services
{
    public class VecTopService
    {
        private readonly ILogger<VecTopService> _logger;
        private readonly BundestagMineDbContext _db;
        private readonly string _vecTopApiBaseUrl;
        private readonly string _vecTopExtractEndpoint;

        public VecTopService(BundestagMineDbContext db, ILogger<VecTopService> logger)
        {
            _logger = logger;
            _db = db;
            _vecTopApiBaseUrl = ConfigManager.GetVecTopBaseUrl();
            _vecTopExtractEndpoint = ConfigManager.GetVecTopExtractEndpoint();
        }

        /// <summary>
        /// This is a cleanup function. We look at all speeches that dont have a category yet,
        /// and add it via VecTop
        /// </summary>
        public async Task ExtractCategoryOfAllSpeeches()
        {
            var take = 50;
            var totalSpeeches = _db.Speeches.Count();
            for (int skip = 0; skip < totalSpeeches; skip += take)
            {
                var speeches = _db.Speeches.Skip(skip).Take(take)
                    .Where(s => !string.IsNullOrEmpty(s.Text) && !_db.Categories.Any(c => c.NLPSpeechId == s.Id))
                    .ToList();
                foreach (var speech in speeches)
                {
                    try
                    {
                        var categories = await CategorizeSingleSpeech(speech);
                        await _db.Categories.AddRangeAsync(categories);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error trying to categorize speech with id: " + speech.Id);
                    }
                }
                await _db.SaveChangesAsync();
                _logger.LogInformation($"Stored categories for {take} more speeches.");
            }
        }

        /// <summary>
        /// Categorisez a single speech with VecTop
        /// </summary>
        /// <param name="speech"></param>
        public async Task<List<Category>> CategorizeSingleSpeech(Speech speech)
        {
            // We categorize a speech by calling the VecTop API
            var url = _vecTopApiBaseUrl;
            _logger.LogInformation("New API request to pixabay with url: " + url);
            var categories = new List<Category>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);

                    // Add an Accept header for JSON format.
                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                    var postData = new Dictionary<string, string>
                    {
                        { "text", speech.Text },
                        { "lang", "de-DE" },
                        { "corpus", "spiegel_sum_1"} // Since we have german speeches, we take the spiegel corpus
                    };
                    var content = new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(_vecTopExtractEndpoint, content);

                    if (response.IsSuccessStatusCode)
                    {
                        // Parse the response body.
                        // Ik we could use a dto but hell, this API returns the most nested arrays I've ever seen.
                        // Lets just do it this way...
                        var answer = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                        var topics = answer?.result.topics;
                        if (topics == null) return categories;
                        foreach (var topic in topics)
                        {
                            // If there arent any subtopics, just store the one category
                            if (((IEnumerable<dynamic>)topic[1]).Count() == 0)
                            {
                                var category = new Category()
                                {
                                    NLPSpeechId = speech.Id,
                                    Name = topic[0],
                                };
                                categories.Add(category);
                            }
                            // Else we create a category foreach subcategory
                            else
                            {
                                foreach (var subTopic in topic[1])
                                {
                                    var category = new Category()
                                    {
                                        NLPSpeechId = speech.Id,
                                        Name = topic[0],
                                        SubCategory = (string)subTopic
                                    };
                                    categories.Add(category);
                                }
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError($"The api call to url {url} failed with statuscode: {(int)response.StatusCode} because {response.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error trying categorize a speech: ");
            }
            return categories;
        }
    }
}
