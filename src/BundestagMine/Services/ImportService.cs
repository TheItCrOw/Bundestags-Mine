using BundestagMine.SqlDatabase;
using BundestagMine.ViewModels.Import;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BundestagMine.Services
{
    public class ImportService
    {
        public ImportLogViewModel BuildImportLogViewModel(string file, bool buildLogLines = false)
        {
            var filename = file.Substring(ConfigManager.GetImportLogOutputPath().Length);
            filename = filename.Substring(0, filename.Length - 4);

            var status = Status.Success;
            var content = File.ReadAllLines(file).ToList();
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

        public List<ImportLogViewModel> GetAllImportLogFileNames()
        {
            var result = new List<ImportLogViewModel>();

            foreach(var file in Directory.GetFiles(ConfigManager.GetImportLogOutputPath()))
            {
                try
                {
                    result.Add(BuildImportLogViewModel(file));
                }
                catch(Exception)
                {
                    // TODO: Log excetpion
                }
            }

            return result;
        }
    }
}
