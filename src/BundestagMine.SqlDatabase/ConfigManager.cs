using System;
using System.Collections.Generic;
using System.IO;
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

        public static string GetDataDirectoryPath() => _config.GetSection("Paths").GetSection("DataDirectory").Value; 

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
        /// Gets the path to the local directory where the data lies.
        /// </summary>
        /// <returns></returns>
        public static string GetLocalDataDirectory() => "\\\\localnas\\home\\Drive\\Bundestag Mining\\Parliament Sentiment Radar Data\\";
        public static string GetLocalMineDirectory() => "\\\\localnas\\home\\Drive\\Bundestag Mining\\";
    }
}
