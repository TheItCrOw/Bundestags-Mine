using BundestagMine.Logic.RequestModels.Pixabay;
using BundestagMine.SqlDatabase;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Logic.Services
{
    public class PixabayApiService
    {
        private readonly ILogger<PixabayApiService> _logger;
        private readonly string _baseUrl;
        private readonly string _searchParameters;
        private readonly string _apiKey;

        public PixabayApiService(ILogger<PixabayApiService> logger)
        {
            _logger = logger;
            _baseUrl = ConfigManager.GetPixabayBaseUrl();
            _apiKey = ConfigManager.GetPixabayAPIKey();
            _searchParameters = ConfigManager.GetPixabayDefaultParameters();
        }

        /// <summary>
        /// Asks the pixabay api for a image to the given searchterm and returns once randomly.
        /// Returns the image as a searchhit
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public async Task<SearchHit> SearchForImageAsync(string searchTerm, string category)
        {
            var fullUri = _baseUrl + _searchParameters;
            fullUri = fullUri.Replace("{SEARCHTERM}", searchTerm).Replace("{CATEGORY}", category);
            _logger.LogInformation("New API request to pixabay with url: " + fullUri);
            var parameters = _searchParameters
                .Replace("{SEARCHTERM}", searchTerm)
                .Replace("{KEY}", _apiKey)
                .Replace("{CATEGORY}", category); 

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_baseUrl);

                    // Add an Accept header for JSON format.
                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                    // List data response.
                    var response = await client.GetAsync(parameters);
                    if (response.IsSuccessStatusCode)
                    {
                        // Parse the response body.
                        var searchResults = JsonConvert.DeserializeObject<ImageSearchResult>(await response.Content.ReadAsStringAsync());
                        var random = new Random();
                        // Lets take a random image from the best 15 images.
                        var hit = searchResults.Hits.OrderBy(x => x.Views).Take(50).OrderBy(x => random.Next()).First();
                        // We want to store the image, so base64 encode it.
                        var imageBytes = await client.GetByteArrayAsync(hit.LargeImageUrl);
                        var imageAsBase64 = Convert.ToBase64String(imageBytes);
                        hit.LargeImageAsBase64 = imageAsBase64;
                        return hit;
                    }
                    else
                    {
                        _logger.LogError($"The api call to url {fullUri} failed with statuscode: {(int)response.StatusCode} because {response.ReasonPhrase}");
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error trying to make an API call to pixabay: ");
            }
            return null;
        }
    }
}
