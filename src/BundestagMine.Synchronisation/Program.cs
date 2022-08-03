using BundestagMine.Models.Database;
using BundestagMine.Models.Database.MongoDB;
using BundestagMine.MongoDB;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace BundestagMine.Synchronisation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            try
            {
                var import = false;
                var export = false;
                var scrape = true;
                var importExsel = false;
                // Connect to the MongoDB

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
                    //await exporter.ExportDeputies();
                    Console.WriteLine("Exporting networkdata");
                    //await exporter.ExportNetworkData();
                    Console.WriteLine("Exporting Speeches");
                    //await exporter.ExportSpeeches();
                    Console.WriteLine("Exporting NLP Speeches");
                    //await exporter.ExportNLPSpeeches();
                    Console.WriteLine("creating lemmavalues for NamedEntities");
                    await exporter.FillNamedEntitiesFromTokens();
                }

                if (scrape)
                {
                    var scraper = new BundestagScraper();
                    Console.WriteLine("Scraping Abstimmungslisten");
                    //scraper.ExportAbstimmungslisten();
                    Console.WriteLine("Scraping Tagesordnungspunkte");
                    scraper.ExportAndImportTagesordnungspunkte();
                }

                if (importExsel)
                {
                    var excelImporter = new ExcelImporter();
                    Console.WriteLine("XLSX");
                    //excelImporter.ImportXLSXAbstimmungslisten();
                    Console.WriteLine("XLS");
                    //excelImporter.ImportXLSAbstimmungslisten();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
