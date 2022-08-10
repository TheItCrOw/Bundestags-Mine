using BundestagMine.Models.Database;
using BundestagMine.Models.Database.MongoDB;
using BundestagMine.MongoDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Serilog;
using System;
using System.Threading.Tasks;

namespace BundestagMine.Synchronisation
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a new logger
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File($"import_logs\\Import_{DateTime.Now.ToShortDateString()}.txt")
            .CreateLogger();

            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            Log.Information("Starting a new import run now at " + DateTime.Now);

            Log.Information("============================================================================");
            Log.Information("Checking new IMPORTED ENTITIES");
            Log.Information("============================================================================");
            var parser = new ImportedEntityParser();
            var result = await parser.ParseNewPotentialEntities();
            if (result == 0) ; // Send a mail, idk. Inform somehow!

            Log.Information("============================================================================");
            Log.Information("Checking new AGENDA ITEMS");
            Log.Information("============================================================================");
            var scraper = new BundestagScraper();
            result = scraper.FetchNewAgendaItems();
            if (result == 0) ; // Send a mail, idk. Inform somehow!

            Log.Information("============================================================================");
            Log.Information("Checking new POLLS");
            Log.Information("============================================================================");
            result = scraper.ExportAbstimmungslisten();
            if (result == 0) ; // Send a mail, idk. Inform somehow!

            return;
            try
            {
                var import = false;
                var export = false;
                var scrape = false;
                var importExsel = false;

                if (import)
                {
                    var importer = new MongoDBImporter(null);
                    importer.Import("Protocols");
                    importer.Import("Deputies");
                    importer.Import("NetworkData");
                    importer.Import("Speeches");
                }

                if (export)
                {
                    var exporter = new MongoDBExporter(null);
                    Console.WriteLine("Exporting deputies");
                    await exporter.ExportDeputies();
                    Console.WriteLine("Exporting networkdata");
                    await exporter.ExportNetworkData();
                    Console.WriteLine("Exporting Speeches");
                    await exporter.ExportSpeeches();
                    Console.WriteLine("Exporting NLP Speeches");
                    await exporter.ExportNLPSpeeches();
                    Console.WriteLine("creating lemmavalues for NamedEntities");
                    await exporter.FillNamedEntitiesFromTokens();
                }

                if (scrape)
                {
                    //var scraper = new BundestagScraper();
                    Console.WriteLine("Scraping Abstimmungslisten");
                    scraper.ExportAbstimmungslisten();
                    Console.WriteLine("Scraping Tagesordnungspunkte");
                    scraper.FetchNewAgendaItems();
                }

                if (importExsel)
                {
                    var excelImporter = new ExcelImporter();
                    Console.WriteLine("XLSX");
                    excelImporter.ImportXLSXAbstimmungslisten();
                    Console.WriteLine("XLS");
                    excelImporter.ImportXLSAbstimmungslisten();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
