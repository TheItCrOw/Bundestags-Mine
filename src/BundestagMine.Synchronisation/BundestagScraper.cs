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
        public void FetchNewAgendaItems()
        {
            // Its going to be a big boi method hehe
            // At the end we need something like:
            // https://www.bundestag.de/apps/plenar/plenar/conferenceweekDetail.form?limit=1&week=2&year=2022
            var url = "https://www.bundestag.de/apps/plenar/plenar/conferenceweekDetail.form?limit=1";
            Log.Information("Scraping TOP from: " + url);

            using (var sqlDb = new BundestagMineDbContext(ConfigManager.GetDbOptions()))
            {
                // We are only interested in the datetime between 2017 and today. Start is at 2017
                // And There are 52 weeks in year
                for (int year = 2022; year <= DateTime.Now.Year; year++)
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
                            if(sqlDb.AgendaItems.Any(a => a.ProtocolId == protocol.Id))
                            {
                                Log.Information($"Already has agendaitems added.");
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
                                    Log.Warning("CAUTION: This protocol has more top than anticipated. Error somewhere?!");
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

                            if(counter < protocol.AgendaItemsCount)
                            {
                                Log.Warning("CAUTION: This protocol has less top than anticipated. Error somewhere?!");
                            }
                            sqlDb.SaveChanges();
                            Log.Information($"Stored new agendaitems.");
                        }
                    }
            }
        }

        /// <summary>
        /// Exports the excels abstimmungslisten to local directry
        /// </summary>
        public void ExportAbstimmungslisten()
        {
            var url = "https://www.bundestag.de/ajax/filterlist/de/parlament/plenum/abstimmung/liste/462112-462112?limit=30&noFilterSet=true&offset=";
            var excelUrl = "https://www.bundestag.de";
            // NO more than 790 it seems.
            for (int i = 0; i < 790; i += 30)
            {
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
                                Console.WriteLine("Kein Datum.");
                            }
                        }
                        // Title and actual excel
                        else if (counter == 2)
                        {
                            // Get the excel doc
                            var a = td.GetElementsByTag("a");
                            if (a.Count < 2)
                            {
                                Console.WriteLine($"Offset {i}: Hatte kein excel download.");
                                break;
                            }

                            // Get title
                            var fullTitle = date + "_" + td.GetElementsByTag("strong")?.First.Html; // has the date
                            var path = ConfigManager.GetLocalMineDirectory() + "Abstimmungslisten\\"
                                    + ReplaceInvalidChars(fullTitle) + "." + a[1].Html.Split("|")[0].ToLower(); // XLSX | 53KB
                            if (File.Exists(path))
                            {
                                Console.WriteLine("Überspringen...");
                                break;
                            }
                            var title = fullTitle.Substring(11, fullTitle.Length - 11).Trim();

                            var blob = a[1].Attr("href");
                            var href = excelUrl + blob;

                            // Create a new WebClient instance.
                            using (var myWebClient = new WebClient())
                            {
                                // Download the Web resource and save it into the current filesystem folder.
                                myWebClient.DownloadFile(href, path);
                                Console.WriteLine("Downloaded " + fullTitle + ".");
                            }
                        }
                        counter++;
                    }
                }
                Console.WriteLine("Offset=" + i);
                Thread.Sleep(3000);
            }
        }
    }
}
