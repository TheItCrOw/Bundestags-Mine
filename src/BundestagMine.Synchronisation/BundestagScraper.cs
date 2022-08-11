using BundestagMine.Models.Database.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.Utility;
using Serilog;
using Supremes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace BundestagMine.Synchronisation
{
    /// <summary>
    /// Everything we fetch from the bundestag page
    /// </summary>
    public class BundestagScraper
    {
        /// <summary>
        /// Replaces invalid characters for a windows filepath
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string ReplaceInvalidChars(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        /// <summary>
        /// Exports the tagesordnungspunkte by scraping and then imports them into the database.
        /// </summary>
        public int FetchNewAgendaItems()
        {
            // Its going to be a big boi method hehe
            // At the end we need something like:
            // https://www.bundestag.de/apps/plenar/plenar/conferenceweekDetail.form?limit=1&week=2&year=2022
            var url = ConfigManager.GetAgendaItemsScrapeUrl();
            Log.Information("Scraping TOP from: " + url);

            try
            {
                using (var sqlDb = new BundestagMineDbContext(ConfigManager.GetDbOptions()))
                {
                    // We are only interested in the datetime between 2017 and today. Start is at 2017
                    // And There are 52 weeks in year
                    for (int year = ConfigManager.GetAgendaItemScrapeStartYear(); year <= DateTime.Now.Year; year++)
                        for (int week = 1; week <= 52; week++)
                        {
                            var curUrl = $"{url}&week={week}&year={year}";
                            var body = Dcsoup.Parse(new Uri(curUrl), 10000);
                            Log.Information($"New Year {year} and week {week}.");
                            // First check if there are any top at this week.
                            var firstDiv = body.GetElementsByClass("bt-standard-content").First;
                            if (firstDiv.Html == "")
                            {
                                Log.Information($"No content.");
                                continue;
                            }

                            // Otherwise, fetch some TOP.
                            var tables = firstDiv.GetElementsByTag("table");
                            // Each table stands for one protocol and has TOPs.
                            foreach (var topTable in tables)
                            {
                                // Fetch some caption data.
                                var caption = topTable.GetElementsByTag("caption").First.GetElementsByClass("bt-conference-title").First.Html;
                                // Its like '6. April 2022 (27. Sitzung)' and we want to split that data
                                var splited = caption.Split(new string[] { "(", ")" }, StringSplitOptions.None);

                                // To a regular fucking date.
                                var date = DateTime.Parse(splited[0]);
                                // (27. Sitzung) to just 27 as int.
                                var protocolNumber = int.Parse(splited[1].Split('.')[0]);

                                // We need to fetch the protocol to store its id with the top.
                                var protocol = sqlDb.Protocols.FirstOrDefault(p => DateTime.Compare(date, p.Date) == 0);
                                // If there is no protocol at that date it means we dont have it cause its older than oktober 2017.
                                if (protocol == null)
                                {
                                    Log.Information($"No protocol (Too early? Too late?).");
                                    continue;
                                };

                                // Check if this protocol already has added agendaitems.
                                if (sqlDb.AgendaItems.Any(a => a.ProtocolId == protocol.Id))
                                {
                                    Log.Information($"Already has agenda items added.");
                                    continue;
                                }

                                // Now go through each top of this protocol table
                                var counter = 1;
                                foreach (var topRow in topTable.GetElementsByTag("tbody").First.GetElementsByTag("tr"))
                                {
                                    // 0: Uhrzeit, 1: TOP, 2: Thema, 3: Status/Abstimmung.
                                    var columns = topRow.GetElementsByTag("td");

                                    // Check if this row is a valid top
                                    var topNumber = columns[1].GetElementsByTag("p").First.Html;
                                    // If not, skipt it.
                                    if (string.IsNullOrEmpty(topNumber)) continue;

                                    if (counter > protocol.AgendaItemsCount)
                                    {
                                        Log.Warning("CAUTION: This protocol has more top than anticipated!");
                                    }

                                    var title = columns[2].GetElementsByClass("bt-top-collapser").First.Html;

                                    // Else, create the agendaitem
                                    var agendaItem = new AgendaItem()
                                    {
                                        ProtocolId = protocol.Id,
                                        Date = date + TimeSpan.Parse(columns[0].GetElementsByTag("p").First.Html),
                                        AgendaItemNumber = topNumber,
                                        Order = counter,
                                        Title = title,
                                        Description = columns[2].GetElementsByClass("bt-top-collapse").First.GetElementsByTag("p").First.Html
                                    };

                                    counter++;
                                    sqlDb.AgendaItems.Add(agendaItem);
                                }

                                if (counter < protocol.AgendaItemsCount)
                                {
                                    Log.Warning("CAUTION: This protocol has less top than anticipated!");
                                }

                                sqlDb.SaveChanges();
                                Log.Information($"Stored new agendaitems to protocol \"{protocol.Title}\".");
                            }
                        }
                }
                return 1;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unknown error while fetching new Agenda items!");
                return 0;
            }
        }

        /// <summary>
        /// Exports the excels abstimmungslisten to local directry
        /// </summary>
        public int ExportAbstimmungslisten()
        {
            var url = ConfigManager.GetPollsScrapeUrl();
            var excelUrl = ConfigManager.GetBundestagUrl();
            Log.Information($"Checking for new excel polls at {url}.");

            try
            {
                using (var db = new BundestagMineDbContext(ConfigManager.GetDbOptions()))
                {
                    // NO more than total of 790 it seems.
                    // We dont want to check ALL lists everytime we import. I think 2 pages should be enough
                    for (int i = 0; i < ConfigManager.GetPollExporterMaxOffset(); i += 30)
                    {
                        Log.Information($"Checking excel polls with offset {i}.");
                        var curUrl = url + i;
                        var body = Dcsoup.Parse(new Uri(curUrl), 10000);
                        var documents = body.GetElementsByTag("tr");
                        foreach (var tr in documents)
                        {
                            var tds = tr.GetElementsByTag("td");
                            var counter = 0;
                            var date = "01.01.2009"; // Everything before with this date can be 2009 or less. We dont know sadly
                            foreach (var td in tds)
                            {
                                // First the date
                                if (counter == 0)
                                {
                                    try
                                    {
                                        date = DateTime.Parse(td.GetElementsByTag("p")?.First.Html).ToShortDateString();
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Warning("Found poll with no date... Check it by hand if you must!", ex);
                                    }
                                }
                                // Title and actual excel
                                else if (counter == 2)
                                {
                                    // Get the excel doc
                                    var a = td.GetElementsByTag("a");
                                    if (a.Count < 2)
                                    {
                                        Log.Information($"Offset {i}: Poll had no excel download. This occurs often with very old polls.");
                                        break;
                                    }

                                    // Get title
                                    var fullTitle = date + "_" + td.GetElementsByTag("strong")?.First.Html; // has the date
                                    var path = ConfigManager.GetDataPollsDirectoryPath()
                                            + ReplaceInvalidChars(fullTitle) + "." + a[1].Html.Split("|")[0].ToLower(); // XLSX | 53KB

                                    // The name format on the urls is a bit tricky... We cannot check if the exact title already
                                    // exists, so check if the date matches and the fullTile contains the stored title. Then we should
                                    // probably already own that poll. Its a bit hacky, ngl
                                    if (db.Polls.Any(p => p.Date.Equals(date) && fullTitle.Contains(p.Title)))
                                    {
                                        Log.Information($"{fullTitle} already in database. Skipping it.");
                                        break;
                                    }

                                    Log.Information($"Trying to download: {fullTitle}");

                                    var blob = a[1].Attr("href");
                                    var href = excelUrl + blob;

                                    // Create a new WebClient instance.
                                    using (var myWebClient = new WebClient())
                                    {
                                        // Download the Web resource and save it into the current filesystem folder.
                                        myWebClient.DownloadFile(href, path);
                                        Log.Information($"Downloaded: {fullTitle}");
                                    }
                                }
                                counter++;
                            }
                        }
                        Thread.Sleep(3000);
                    }
                }

                return 1;
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Unknown error while trying to export polls!");
                return 0;
            }            
        }
    }
}
