using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace BundestagMine.SqlDatabase
{
    public class ConfigManager
    {
        private static IConfigurationRoot _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        /// <summary>
        /// Gets the connection string for the sql database
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString() => _config.GetSection("ConnectionStrings").GetSection("DefaultConnection").Value;
        public static string GetTokenDbConnectionString() => _config.GetSection("ConnectionStrings").GetSection("TokenConnection").Value;

        public static string GetDataDirectoryPath() => _config.GetSection("Paths").GetSection("DataDirectory").Value;

        public static string GetDataPollsDirectoryPath() => _config.GetSection("Paths").GetSection("PollsDirectory").Value;

        public static int GetPollExporterMaxOffset() =>
            int.Parse(_config.GetSection("Configurations").GetSection("PollsExportMaxOffset").Value);

        public static int GetAgendaItemScrapeStartYear() =>
            int.Parse(_config.GetSection("Configurations").GetSection("AgendaItemScrapeStartYear").Value);

        public static string GetAgendaItemsScrapeUrl() => _config.GetSection("Configurations").GetSection("AgendaItemsScrapeUrl").Value;
        public static string GetPollsScrapeUrl() => _config.GetSection("Configurations").GetSection("PollsScrapeUrl").Value;
        public static string GetBundestagUrl() => _config.GetSection("Configurations").GetSection("BundestagUrl").Value;
        public static string GetPortraitDatabaseQueryUrl() => _config.GetSection("Configurations").GetSection("PortraitDatabaseQueryUrl").Value;
        public static string GetPortraitDatabaseUrl() => _config.GetSection("Configurations").GetSection("PortraitDatabaseUrl").Value;
        public static string GetCachedPortraitPath() => _config.GetSection("Configurations").GetSection("CachedPortraitPath").Value;
        public static string GetBase64SourcePrefix() => _config.GetSection("Configurations").GetSection("Base64SourcePrefix").Value;
        public static string GetPollsQueryUrl() => _config.GetSection("Configurations").GetSection("PollsQueryUrl").Value;
        public static bool GetDeleteImportedEntity() => bool.Parse(_config.GetSection("Configurations").GetSection("DeleteImportedEntity").Value);
        public static string GetImportLogOutputPath() => _config.GetSection("Configurations").GetSection("ImportLogOutputPath").Value;
        public static List<string> GetImportReportRecipients() => _config.GetSection("Configurations").GetSection("ImportReportRecipients").Value.Split(',').ToList();

        public static string GetSmtpHost() => _config.GetSection("Smtp").GetSection("Host").Value;
        public static int GetSmtpPort() => int.Parse(_config.GetSection("Smtp").GetSection("Port").Value);
        public static string GetSmtpUsername() => _config.GetSection("Smtp").GetSection("Username").Value;
        public static string GetSmtpPassword() => _config.GetSection("Smtp").GetSection("Password").Value;
        public static bool GetSmtpEnableSSL() => bool.Parse(_config.GetSection("Smtp").GetSection("EnableSSL").Value);
        public static bool GetSmtpIsBodyHtml() => bool.Parse(_config.GetSection("Smtp").GetSection("IsBodyHtml").Value);
        public static string GetGenericMailTemplatePath() => _config.GetSection("MailTemplates").GetSection("GenericMailTemplatePath").Value;
        public static string GetGenericMailTemplateWithButtonPath() => _config.GetSection("MailTemplates").GetSection("GenericMailTemplateWithButtonPath").Value;

        public static string GetDownloadCenterCalculatingDataDirectory() =>
            _config.GetSection("Configurations").GetSection("DownloadCenter").GetSection("CalculatingDataDirectory").Value;
        
        public static string GetDownloadCenterFinishedZippedDataSetsDirectory() =>
            _config.GetSection("Configurations").GetSection("DownloadCenter").GetSection("FinishedZippedDataSetsDirectory").Value;

        public static string GetPresetDatasetsLastUpdateDate() =>
            _config.GetSection("Configurations").GetSection("DownloadCenter").GetSection("PresetDatasetsLastUpdateDate").Value;

        public static string GetPixabayAPIKey() => _config.GetSection("PixabayAPI").GetSection("APIKey").Value;
        public static string GetPixabayBaseUrl() => _config.GetSection("PixabayAPI").GetSection("BaseUrl").Value;
        public static string GetPixabayDefaultParameters() => _config.GetSection("PixabayAPI").GetSection("DefaultParameters").Value;

        /// <summary>
        /// Returns the db options we need to pass into each new db context.
        /// </summary>
        /// <returns></returns>
        public static DbContextOptions<BundestagMineDbContext> GetDbOptions()
        {
            var options = new DbContextOptionsBuilder<BundestagMineDbContext>();
            var conn = GetConnectionString();
            options.UseSqlServer(conn, o => o.CommandTimeout(600));
            return options.Options;
        }

        /// <summary>
        /// Returns the db options we need to pass into each new token db context.
        /// </summary>
        /// <returns></returns>
        public static DbContextOptions<BundestagMineTokenDbContext> GetTokenDbOptions()
        {
            var options = new DbContextOptionsBuilder<BundestagMineTokenDbContext>();
            var conn = GetTokenDbConnectionString();
            options.UseSqlServer(conn, o => o.CommandTimeout(600));
            return options.Options;
        }

        /// <summary>
        /// Gets the path to the local directory where the data lies.
        /// </summary>
        /// <returns></returns>
        public static string GetLocalDataDirectory() => "\\\\localnas\\home\\Drive\\Bundestag Mining\\Parliament Sentiment Radar Data\\";
        public static string GetLocalMineDirectory() => "\\\\localnas\\home\\Drive\\Bundestag Mining\\";
    }
}
