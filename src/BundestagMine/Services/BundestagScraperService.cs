using BundestagMine.Models.Database;
using BundestagMine.Models.Database.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.Utility;
using Microsoft.Extensions.Logging;
using Supremes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace BundestagMine.Services
{
    public class BundestagScraperService
    {
        private readonly ILogger<BundestagScraperService> _logger;
        private readonly BundestagMineDbContext _db;

        public BundestagScraperService(BundestagMineDbContext db, ILogger<BundestagScraperService> logger)
        {
            _logger = logger;
            _db = db;
        }

        private static string RemoveWhitespaces(string str)
            => string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));

        /// <summary>
        /// Removes the span, the spaces, tolower
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string ToCleanTitle(string s) => Regex.Replace(RemoveWhitespaces(HttpUtility.HtmlDecode(s).ToLower()), @"<[^>]*>", String.Empty);

        public string GetBundestagUrlOfPoll(Poll poll)
        {
            var url = ConfigManager.GetPollsQueryUrl();
            // sample page:
            // https://www.bundestag.de/ajax/filterlist/de/parlament/plenum/abstimmung/484422-484422?enddate=1629849600000&endfield=date&limit=1000&noFilterSet=false&startdate=1629849600000&startfield=date

            // The Date in the poll can differ from the protocol which fucks up the search.
            // So take the protocols date instead of the polls date.
            var protocol = _db.Protocols.FirstOrDefault(p => p.LegislaturePeriod == poll.LegislaturePeriod && p.Number == poll.ProtocolNumber);

            var from = poll.Date;
            var to = poll.Date;
            if (protocol?.Date > poll.Date) to = protocol.Date;
            else if (protocol?.Date < poll.Date) from = protocol.Date;

            // We need the date as milliseconds as the from and to date for the url.
            var curUrl = url.Replace("%ENDDATE%", ((long)(to - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString());
            curUrl = curUrl.Replace("%STARTDATE%", ((long)(from - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString());
            var body = Dcsoup.Parse(new Uri(curUrl), 10000);
            var polls = body.GetElementsByClass("col-xs-12");
            var levenshtein = new Levenshtein();
            var editToSites = new List<(int, string)>();

            // Each div has a name. If the name mathces our title, get the url of the <a>
            foreach (var pollDiv in polls)
            {
                var titleH3 = pollDiv.GetElementsByClass("bt-teaser-text")?.First?.GetElementsByTag("h3")?.First;
                if (titleH3 == null) continue;

                var title = DateHelper.ReplaceInvalidPathChars(ToCleanTitle(titleH3.Html));
                var edits = levenshtein.Compute(title, ToCleanTitle(poll.Title));
                // Lets just take all polls into consideration and take the one, which is closest...
                // Now get the href.
                var href = ConfigManager.GetBundestagUrl() + pollDiv.GetElementsByTag("a").First?.Attr("href");
                editToSites.Add((edits, href));
            }

            // If there are multiple hits, we want the hit with the least edits required. Thats the nearest we get.
            if (editToSites.Count > 0) return editToSites.OrderBy(e => e.Item1).First().Item2;

            return string.Empty;
        }

        public string GetDeputyPortraitFromImageDatabase(string speakerId)
            => GetDeputyPortraitFromImageDatabase(_db.Deputies.FirstOrDefault(d => d.SpeakerId == speakerId));

        public string GetDeputyPortraitFromImageDatabase(Deputy deputy)
        {
            // We check if we have fetched the image already onto our harddrive. If yes, then fetch it from there
            // and return it as a base64 string. Else fetch it from the bundestag website and in parallel cache it.
            var filename = ConfigManager.GetCachedPortraitPath() + deputy.SpeakerId + ".jpg";
            if (File.Exists(filename))
            {
                var imageArray = File.ReadAllBytes(filename);
                return ConfigManager.GetBase64SourcePrefix() + Convert.ToBase64String(imageArray);
            }
            // Else scrape it, store it, return it
            var name = (deputy.FirstName + "+" + deputy.LastName);
            var url = ConfigManager.GetPortraitDatabaseQueryUrl() + name;
            var imagesOverview = Dcsoup.Parse(new Uri(url), 3000);
            var test = imagesOverview.GetElementsByClass("rowGridContainer");
            var container = imagesOverview.GetElementsByClass("rowGridContainer")[0];
            var elements = container.GetElementsByClass("item");
            if (elements.Count != 0)
            {
                var imgTag = elements[0].GetElementsByTag("img")[0];
                var imgUrl = ConfigManager.GetPortraitDatabaseUrl() + imgTag.Attr("src");

                try
                {
                    // We want this to run async in the background
                    Task.Run(() => DownloadAndStoreDeputyPortrait(imgUrl, deputy.SpeakerId));
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error storing new deputy portraits.");
                }

                return imgUrl;
            }

            return string.Empty;
        }


        /// <summary>
        /// Takes in a img url and the speaker id and stores the img on our harddrive with less resolution
        /// </summary>
        public void DownloadAndStoreDeputyPortrait(string imgUrl, string speakerId)
        {
            using (var client = new WebClient())
            {
                try
                {
                    // Download the file
                    var filename = ConfigManager.GetCachedPortraitPath() + speakerId + ".jpg";

                    if (!File.Exists(filename))
                        client.DownloadFile(new Uri(imgUrl), filename);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error downloading and storing deputy portrait:");
                }
            }
        }
    }
}
