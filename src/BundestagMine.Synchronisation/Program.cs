using BundestagMine.Models.Database;
using BundestagMine.Models.Database.MongoDB;
using BundestagMine.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.Synchronisation.Services;
using BundestagMine.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;
using Serilog;
using Serilog.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BundestagMine.Synchronisation
{
    class Program
    {
        // We need a englisch format date, since the IIS on the server only works with those...
        private static string _fullLogFileName = $"{ConfigManager.GetImportLogOutputPath()}{DateTime.Now.ToString("yyyy-MM-dd")}.txt";

        static void Main(string[] args)
        {
            // Creates a log set of 20 MB files like:
            //   log.txt
            //   log_001.txt
            //   log_002.txt
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithExceptionDetails()
                .WriteTo.File(_fullLogFileName, rollOnFileSizeLimit: true, fileSizeLimitBytes: 20_971_520)
            .CreateLogger();

            try
            {
                MainAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unknown error canceled the whole Import!");
                Log.CloseAndFlush();

                // Send a mail about the failure.
                MailManager.SendMail($"Import-Abbruch!",
                    $"Der Import wurde um {DateTime.Now} komplett abgebrochen! Log ist im Anhang.",
                    ConfigManager.GetImportReportRecipients(),
                    new List<Attachment> { new Attachment(_fullLogFileName) });
            }
        }

        private static async Task MainAsync()
        {
            MigrateTokenFromDefaultDbToTokenDb();
            return;

            var curDate = DateTime.Now;
            MailManager.SendMail($"Import-Start um {curDate}",
                $"Der Entity-Import startet jetzt.",
                ConfigManager.GetImportReportRecipients());

            Log.Information("Starting a new import run now at " + curDate);
            Log.Information("\n");

            Log.Information("============================================================================");
            Log.Information("Checking new IMPORTED ENTITIES");
            Log.Information("============================================================================");
            var parser = new ImportedEntityParser();
            var entityResult = await parser.ParseNewPotentialEntities();

            Log.Information("============================================================================");
            Log.Information("Checking new AGENDA ITEMS");
            Log.Information("============================================================================");
            var scraper = new BundestagScraper();
            var agendaItemResult = scraper.FetchNewAgendaItems();

            Log.Information("============================================================================");
            Log.Information("Checking new POLLS");
            Log.Information("============================================================================");
            Log.Information("Exporting polls from Bundestag...");
            var exportPollsResult = scraper.ExportAbstimmungslisten();

            Log.Information("Importing polls into database...");
            var importer = new ExcelImporter();
            Log.Information("XLSX ======================================");
            importer.ImportXLSXPolls();
            Log.Information("XLS  ======================================");
            importer.ImportXLSPolls();

            if (CacheService.NewProtocolsStored == 0)
            {
                Log.Information("No new entities imported, therefore no graph calculations need to be done.");
            }
            else
            {
                Log.Information("============================================================================");
                Log.Information("Recalculate GRAPHS");
                Log.Information("============================================================================");
                var graphService = new GraphService();

                Log.Information("Topic Bar Race Chart ======================================");
                var graphData = graphService.BuildTopicBarRaceChartData();
                if (graphData != null)
                {
                    var dataString = JsonConvert.SerializeObject(graphData);
                    var path = $"{ConfigManager.GetDataDirectoryPath()}topicBarRaceData.json";
                    File.WriteAllText(path, dataString);
                    Log.Information($"Stored new Topic Bar Race Chart Data under {path}");
                }

                Log.Information("Topic Map ======================================");
                var from = DateTime.Parse($"01.01.{DateTime.Now.Year}");
                var to = DateTime.Parse($"31.12.{DateTime.Now.Year}");
                Log.Information($"Year: {from.Year}");
                var data = graphService.BuildTopicMapData(from, to);
                if (data != null)
                {
                    var dataString = JsonConvert.SerializeObject(data);
                    File.WriteAllText($"{ConfigManager.GetDataDirectoryPath()}topicMap_{from.Year}.json", dataString);
                    Log.Information($"Stored the topic map data!");
                }

                from = DateTime.Parse($"01.01.2017");
                to = DateTime.Parse($"31.12.{DateTime.Now.AddYears(1).Year}");
                Log.Information($"Year: Gesamt");
                data = graphService.BuildTopicMapData(from, to);
                if (data != null)
                {
                    var dataString = JsonConvert.SerializeObject(data);
                    File.WriteAllText($"{ConfigManager.GetDataDirectoryPath()}topicMap_Gesamt.json", dataString);
                    Log.Information($"Stored the topic map data!");
                }

                // Comment Network
                Log.Information("Comment Network ======================================");
                graphService.BuildAndStoreCommentNetworkData();
            }

            Log.Information("============================================================================");
            Log.Information("============================================================================");
            Log.Information($"Done. Import run ended at {DateTime.Now}");
            // We have to dispose the logger, otherwise the log file is being occupied by the process.
            Log.CloseAndFlush();

            // Send a mail about the import.
            MailManager.SendMail($"Import-Bericht {curDate.ToShortDateString()}",
                $"Stati:<br/>Entity-Import: {entityResult}<br/>Agenda-Scrape: {agendaItemResult}<br/>Polls-Scrape: {exportPollsResult}<br/><br/>Log im Anhang.",
                ConfigManager.GetImportReportRecipients(),
                new List<Attachment> { new Attachment(_fullLogFileName) });
        }

        /// <summary>
        /// 2022-01-04: Created a new Database context and db since the default is full. This puts all the tokens
        /// of the default db into the new database.
        /// </summary>
        private static void MigrateTokenFromDefaultDbToTokenDb()
        {
            Log.Information("Starting the token migration:");
            var counter = 0;
            using (var db = new BundestagMineDbContext(ConfigManager.GetDbOptions()))
            {
                var total = db.Token.Count();
                Log.Information($"Found {total} token in the default database");
                using (var tokenDb = new BundestagMineTokenDbContext(ConfigManager.GetTokenDbOptions()))
                {
                    foreach(var tokens in db.Token.OrderBy(t => t.NLPSpeechId).Skip(12000000).QueryChunksOfSize(10000))
                    {
                        foreach(var token in tokens)
                        {
                            if (tokenDb.Token.Contains(token)) continue;
                            tokenDb.Token.Add(token);
                        }
                        tokenDb.SaveChanges();
                        Log.Information("Stored 10000 token in the token db!");
                        counter += 10000;
                        Log.Information($"Token {12000000 + counter}/{total} => {counter/total*100}%");
                    }
                }
            }
        }
    }
}
