using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using BundestagMine.Models.Database;
using BundestagMine.SqlDatabase;
using Serilog;
using Syncfusion.XlsIO;

namespace BundestagMine.Synchronisation
{
    public class ExcelImporter
    {
        public int ImportAllPolls()
        {
            return 1;
        }

        /// <summary>
        /// Imporst the old XLS abstimmungslisten as excel into the mssql
        /// </summary>
        public void ImportXLSPolls()
        {
            using (var sqlDb = new BundestagMineDbContext(ConfigManager.GetDbOptions()))
            {
                // For future imporst: The path may olny take 255 characters. Some excel files are long af
                // So put them into C:\a whatever and let them import there, otherwise ull get errors...
                var xlsFiles = Directory.EnumerateFiles(ConfigManager.GetDataPollsDirectoryPath(), "*.xls");
                Log.Information($"Found {xlsFiles.Count()} new importable xls polls.");

                var counter = 1;
                foreach (var file in xlsFiles)
                {
                    Log.Information($"Importing {counter}/{xlsFiles.Count()}");

                    try
                    {
                        var splited = file.Split("\\");
                        var fileName = splited[splited.Length - 1];
                        var title = fileName.Substring(22, fileName.Length - 26).Trim();
                        Log.Information("Trying to import poll: " + title);

                        if (sqlDb.Polls.Any(p => p.Title == title))
                        {
                            Log.Information("Skipping importing Poll because its already in the database: " + title);
                            continue;
                        }

                        var strExcelConn = "Provider=Microsoft.ACE.OLEDB.12.0;"
                            + $"Data Source={file};"
                            + "Extended Properties='Excel 8.0;HDR=Yes'";

                        var dt = new DataTable();

                        var poll = new Poll()
                        {
                            Date = DateTime.Parse(fileName.Substring(0, 10)),
                            Title = title,
                            Entries = new List<PollEntry>()
                        };

                        using (var cn = new OleDbConnection { ConnectionString = strExcelConn })
                        {
                            cn.Open();
                            var dtExcelSchema = cn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                            var sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                            var selectStatement = $"SELECT * FROM [{sheetName}]";

                            using (var cmd = new OleDbCommand { Connection = cn, CommandText = selectStatement })
                            {
                                dt.Load(cmd.ExecuteReader());
                                // On the A row are the legislature period. Its always the same
                                poll.LegislaturePeriod = int.Parse(dt.Rows[0].ItemArray[0].ToString());
                                poll.ProtocolNumber = int.Parse(dt.Rows[0].ItemArray[1].ToString());
                                poll.PollNumber = int.Parse(dt.Rows[0].ItemArray[2].ToString());

                                for (int row = 1; row < dt.Rows.Count; row++)
                                {
                                    var entry = new PollEntry();
                                    entry.PollId = poll.Id;
                                    entry.Fraction = dt.Rows[row].ItemArray[3].ToString();
                                    entry.LastName = dt.Rows[row].ItemArray[4].ToString();
                                    entry.FirstName = dt.Rows[row].ItemArray[5].ToString();
                                    entry.Yes = dt.Rows[row].ItemArray[6].ToString() == "1" ? true : false;
                                    entry.No = dt.Rows[row].ItemArray[7].ToString() == "1" ? true : false;
                                    entry.Abstention = dt.Rows[row].ItemArray[8].ToString() == "1" ? true : false;
                                    entry.NotValid = dt.Rows[row].ItemArray[9].ToString() == "1" ? true : false;
                                    entry.NotSubmitted = dt.Rows[row].ItemArray[10].ToString() == "1" ? true : false;
                                    entry.Comment = dt.Rows[row].ItemArray[12].ToString();
                                    poll.Entries.Add(entry);
                                }

                                // Save
                                sqlDb.Polls.Add(poll);
                                sqlDb.PollEntries.AddRange(poll.Entries);
                                sqlDb.SaveChanges();
                                Log.Information($"Imported and saved {poll.Title} with {poll.Entries.Count} entries.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error trying to import xls poll: " + ex);
                    }
                    counter++;
                }
            }
        }

        /// <summary>
        /// Imporst the abstimmungslisten as excel into the mssql
        /// </summary>
        public void ImportXLSXPolls()
        {
            using (var sqlDb = new BundestagMineDbContext(ConfigManager.GetDbOptions()))
            {
                using (var excelEngine = new ExcelEngine())
                {
                    var app = excelEngine.Excel;
                    app.DefaultVersion = ExcelVersion.Xlsx;
                    var xlsxFiles = Directory.EnumerateFiles(ConfigManager.GetDataPollsDirectoryPath(), "*.xlsx");
                    Log.Information($"Found {xlsxFiles.Count()} new importable xlsx polls.");

                    foreach (var file in xlsxFiles)
                    {
                        var splited = file.Split("\\");
                        var fileName = splited[splited.Length - 1];
                        var title = fileName.Substring(22, fileName.Length - 26).Trim();
                        Log.Information("Trying to import poll: " + title);

                        if (sqlDb.Polls.Any(p => p.Title == title))
                        {
                            Log.Information("Skipping importing Poll because its already in the database: " + title);
                            continue;
                        }

                        try
                        {
                            var workbook = app.Workbooks.Open(File.Open(file, FileMode.Open), ExcelOpenType.Automatic);
                            //Access the first worksheet
                            var worksheet = workbook.Worksheets[0];

                            var poll = new Poll()
                            {
                                Date = DateTime.Parse(fileName.Substring(0, 10)),
                                Title = title,
                                Entries = new List<PollEntry>()
                            };

                            // On the A row are the legislature period. Its always the same
                            poll.LegislaturePeriod = (int)worksheet["A2"].Number;
                            poll.ProtocolNumber = (int)worksheet["B2"].Number;
                            poll.PollNumber = (int)worksheet["C2"].Number;

                            // Go through all cells and read the values
                            for (int i = 2; i < 9999999; i++)
                            {
                                // Check if there are still rows
                                if (worksheet["D" + i].Text == null) break;

                                var entry = new PollEntry();
                                entry.PollId = poll.Id;
                                entry.Fraction = worksheet["D" + i].DisplayText;
                                entry.LastName = worksheet["E" + i].DisplayText;
                                entry.FirstName = worksheet["F" + i].DisplayText;
                                entry.Yes = worksheet["H" + i].DisplayText == "1" ? true : false;
                                entry.No = worksheet["I" + i].DisplayText == "1" ? true : false;
                                entry.Abstention = worksheet["J" + i].DisplayText == "1" ? true : false;
                                entry.NotValid = worksheet["K" + i].DisplayText == "1" ? true : false;
                                entry.NotSubmitted = worksheet["L" + i].DisplayText == "1" ? true : false;
                                entry.Comment = worksheet["N" + i].DisplayText;
                                poll.Entries.Add(entry);
                            }

                            // Save
                            sqlDb.Polls.Add(poll);
                            sqlDb.PollEntries.AddRange(poll.Entries);
                            sqlDb.SaveChanges();
                            Log.Information($"Imported and saved {poll.Title} with {poll.Entries.Count} entries.");
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Unknown Error with {title}: ", ex);
                        }
                    }
                }
            }
        }
    }
}
