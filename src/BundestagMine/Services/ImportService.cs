using BundestagMine.SqlDatabase;
using BundestagMine.ViewModels.Import;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BundestagMine.Services
{
    /// <summary>
    /// Important: We have to use Streams here and not File.ReadAll() stuff, becuase the serilog logger could
    /// wrwite to the log file when we want to read it, and we have to read it while another process is trying to write.
    /// thats why we use streams here.
    /// </summary>
    public class ImportService
    {
        public List<string> GetLinesFromImportProtocol(int begin, string filePath)
        {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var content = new List<string>();
                var counter = 0;
                using (var streamReader = new StreamReader(stream))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (counter > begin)
                            content.Add(line);
                        counter++;
                    }
                }

                return content;
            }
        }


        /// <summary>
        /// Builds a viewmodel out of a import log txt
        /// </summary>
        /// <param name="file"></param>
        /// <param name="buildLogLines"></param>
        /// <returns></returns>
        public ImportLogViewModel BuildImportLogViewModel(string file, bool buildLogLines = false)
        {
            // PATH\\19.09.22.txt is the format of the textfile
            var filename = file.Substring(ConfigManager.GetImportLogOutputPath().Length);
            filename = filename.Substring(0, filename.Length - 4);

            var status = Status.Success;
            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var content = new List<string>();
                using (var streamReader = new StreamReader(stream))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        content.Add(line);
                    }

                    if (content.Any(c => c.Contains("[WRN]"))) status = Status.Warning;
                    if (content.Any(c => c.Contains("[ERR]"))) status = Status.Error;

                    return new ImportLogViewModel()
                    {
                        Date = DateTime.Parse(filename),
                        Name = filename,
                        FullFilePath = file,
                        Status = status,
                        LogLines = buildLogLines ? content : null
                    };
                }
            }
        }

        public List<ImportLogViewModel> GetAllImportLogFileNames()
        {
            var result = new List<ImportLogViewModel>();

            foreach (var file in Directory.GetFiles(ConfigManager.GetImportLogOutputPath()))
            {
                try
                {
                    result.Add(BuildImportLogViewModel(file));
                }
                catch (Exception)
                {
                    // TODO: Log excetpion
                }
            }

            return result;
        }
    }
}
