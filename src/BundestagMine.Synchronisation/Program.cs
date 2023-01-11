using BundestagMine.Logic.Services;
using BundestagMine.Models.Database;
using BundestagMine.Models.Database.MongoDB;
using BundestagMine.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.Synchronisation.Services;
using BundestagMine.Utility;
using Microsoft.EntityFrameworkCore;
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

        private static IServiceProvider serviceProvider;

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
                // Setup all the services needed for the synchronisation
                IServiceCollection services = new ServiceCollection();
                services.AddLogging(c => c.ClearProviders());
                services.AddTransient<AnnotationService>();
                services.AddTransient<GraphDataService>();
                services.AddTransient<MetadataService>();
                services.AddTransient<BundestagScraperService>();
                services.AddTransient<TopicAnalysisService>();
                services.AddTransient<ImportService>();
                services.AddTransient<GlobalSearchService>();
                services.AddTransient<DownloadCenterService>();
                services.AddTransient<DailyPaperService>();
                services.AddTransient<PixabayApiService>();
                // Add the default db context
                services.AddDbContext<BundestagMineDbContext>(
                    option => option.UseSqlServer(ConfigManager.GetConnectionString(), o => o.CommandTimeout(600)));
                // Add the token db context
                services.AddDbContext<BundestagMineTokenDbContext>(
                    option => option.UseSqlServer(ConfigManager.GetTokenDbConnectionString(), o => o.CommandTimeout(600)));

                serviceProvider = services.BuildServiceProvider();


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

            Log.Information("============================================================================");
            Log.Information("Building the new Daily Papers");
            Log.Information("============================================================================");
            using (var db = new BundestagMineDbContext(ConfigManager.GetDbOptions()))
            {
                //Foreach protocol without a daily paper - build the daily paper.
                foreach (var protocol in serviceProvider.GetService<DailyPaperService>().GetProtocolsWithoutDailyPaper())
                {
                    try
                    {
                        Log.Information($"Creating daily paper for protocol: {protocol.LegislaturePeriod}, " +
                            $"{protocol.Number}");
                        var dailyPaper = BuildDailyPaper(protocol);
                        db.DailyPapers.Add(dailyPaper);
                        db.SaveChanges();
                        Log.Information($"Saved!");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Error building the daily paper for protocol: " +
                            $"{protocol.LegislaturePeriod},{protocol.Number}");
                    }
                }
            }

            // Graphs
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
        /// Builds the daily paper to a given protocol
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        private static DailyPaper BuildDailyPaper(Protocol protocol)
        {
            return new DailyPaper()
            {
                Created = DateTime.Now,
                ProtocolDate = protocol.Date,
                ProtocolNumber = protocol.Number,
                JsonDataString = JsonConvert.SerializeObject(
                    serviceProvider.GetService<DailyPaperService>().BuildDailyPaperViewModel(protocol)),
                LegislaturePeriod = protocol.LegislaturePeriod,
            };
        }
    }
}
