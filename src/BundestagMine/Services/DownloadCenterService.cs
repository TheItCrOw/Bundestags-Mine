using BundestagMine.Models;
using BundestagMine.SqlDatabase;
using BundestagMine.Utility;
using BundestagMine.ViewModels.DownloadCenter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace BundestagMine.Services
{
    public class DownloadCenterService
    {
        private readonly ILogger<DownloadCenterService> _logger;
        private readonly BundestagMineDbContext _db;
        public float AverageFetchTimeInSecondsPerNLPSpeech() => 0.1f;
        public float AverageFileSizeInMBPerNLPSpeech() => 0.025f;

        public DownloadCenterService(BundestagMineDbContext db, ILogger<DownloadCenterService> logger)
        {
            _logger = logger;
            _db = db;
        }

        /// <summary>
        /// Checks if the given dataset is being calculated right now.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool CheckIfDatasetIsBeingCalculated(string filename) => Directory.Exists(ConfigManager.GetDownloadCenterCalculatingDataDirectory() + filename);

        /// <summary>
        /// Searches the dircetory for a finished zip file and creates a viewmodel out of it.
        /// Returns null if the file doesnt exist yet or anymore.
        /// </summary>
        /// <returns></returns>
        public DownloadableZipFileViewModel GetZipFileAsViewModel(string filename)
        {
            var fullFilename = ConfigManager.GetDownloadCenterFinishedZippedDataSetsDirectory() + filename;

            if (!File.Exists(fullFilename))
            {
                var filenameWithoutExtension = Path.ChangeExtension(filename, null);
                // Maybe its being calculated
                if (CheckIfDatasetIsBeingCalculated(filenameWithoutExtension))
                {
                    return new DownloadableZipFileViewModel()
                    {
                        IsBeingCalculated = true
                    };
                }
                // Else, its just not there.                
                return null;
            }

            var fileInfo = new FileInfo(fullFilename);
            return new DownloadableZipFileViewModel()
            {
                Created = fileInfo.CreationTime,
                DeletionTime = fileInfo.CreationTime.AddHours(24),
                FileName = filename,
                SizeInMb = Math.Round(MathHelper.ConvertBytesToMegabytes(fileInfo.Length), 2)
            };
        }

        /// <summary>
        /// We cant store that many protocols in RAM, so we write them onto the disc to park them there.
        /// Returns the fullname of the calculating file.
        /// </summary>
        /// <returns></returns>
        public string WriteDownloadableProtocolsToDisc(
            Guid exportId,
            DateTime from,
            DateTime to,
            List<string> fractions,
            List<string> parties,
            List<string> speakerIds)
        {
            var log = new StringBuilder();
            log.AppendLine("New Dataset Calculation starting at " + DateTime.Now);
            var exportFileName = "";

            try
            {
                log.AppendLine("Export Id: " + exportId);
                exportFileName = $"Export_{exportId}";
                var directoryPath = ConfigManager.GetDownloadCenterCalculatingDataDirectory() + exportFileName;
                Directory.CreateDirectory(directoryPath);

                // Fetch the protocols and speeches, store them onto disc and zip them at the end.
                var protocols = _db.Protocols.Where(p => p.Date >= from && p.Date <= to).AsEnumerable();
                log.AppendLine($"Found {protocols.Count()} protocols");

                foreach (var protocol in protocols)
                {
                    log.AppendLine($"Handling protocol LP: {protocol.LegislaturePeriod} SN: {protocol.Number}");
                    var downloadProtocol = new CompleteDownloadProtocol();
                    downloadProtocol.Protocol = protocol;
                    downloadProtocol.AgendaItems = _db.AgendaItems.Where(ag => ag.ProtocolId == downloadProtocol.Protocol.Id).ToList();
                    downloadProtocol.NLPSpeeches = _db.NLPSpeeches
                        .Where(s => s.LegislaturePeriod == protocol.LegislaturePeriod && s.ProtocolNumber == protocol.Number && _db.Deputies.Any(d => d.SpeakerId == s.SpeakerId)
                            && (
                            speakerIds.Contains(s.SpeakerId)
                            || fractions.Contains(_db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId).Fraction)
                            || parties.Contains(_db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId).Party)
                            ))
                        .AsEnumerable()
                        .Select(s =>
                        {
                            s.Sentiments = _db.Sentiment.Where(t => t.NLPSpeechId == s.Id).ToList();
                            s.NamedEntities = _db.NamedEntity.Where(t => t.NLPSpeechId == s.Id).ToList();
                            s.Segments = _db.SpeechSegment.Where(ss => ss.SpeechId == s.Id).Include(ss => ss.Shouts).ToList();
                            return s;
                        })
                        .ToList();

                    log.AppendLine($"Generated the Model, now json and save it.");
                    // Generate a json
                    var asJson = JsonConvert.SerializeObject(downloadProtocol);
                    log.AppendLine($"Json done");
                    // put it into the zip file
                    var filename = $"LP_{protocol.LegislaturePeriod}_Sitzung_{protocol.Number}.json";
                    File.WriteAllText(Path.Combine(directoryPath, filename), asJson);
                    log.AppendLine($"Writing it to " + filename);
                }
            }
            catch (Exception ex)
            {
                log.AppendLine("Error! Should be logged above this block");
                _logger.LogError(ex, "Error while trying to calculate a dataset");
            }
            finally
            {
                log.AppendLine("Ended at " + DateTime.Now);
                _logger.LogInformation(log.ToString());
            }

            return exportFileName;
        }

        /// <summary>
        /// Takes in filter params and generates the complete nlp protocols, makes them to json, zips them and returns the zip file as bytes.
        /// </summary>
        /// <returns></returns>
        public byte[] GetCompleteDownloadableProtocolsAsZIP(
            DateTime from,
            DateTime to,
            List<string> fractions,
            List<string> parties,
            List<string> speakerIds)
        {
            var protocols = _db.Protocols.Where(p => p.Date >= from && p.Date <= to).AsEnumerable();

            // The stream that prints the bytes into the return bytes
            using (var outStream = new MemoryStream())
            {
                // The actual zip archive stream
                using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
                {
                    foreach (var protocol in protocols)
                    {
                        var downloadProtocol = new CompleteDownloadProtocol();
                        downloadProtocol.Protocol = protocol;
                        downloadProtocol.AgendaItems = _db.AgendaItems.Where(ag => ag.ProtocolId == downloadProtocol.Protocol.Id).ToList();
                        downloadProtocol.NLPSpeeches = _db.NLPSpeeches
                            .Where(s => s.LegislaturePeriod == protocol.LegislaturePeriod && s.ProtocolNumber == protocol.Number && _db.Deputies.Any(d => d.SpeakerId == s.SpeakerId)
                                && (
                                speakerIds.Contains(s.SpeakerId)
                                || fractions.Contains(_db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId).Fraction)
                                || parties.Contains(_db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId).Party)
                                ))
                            .AsEnumerable()
                            .Select(s =>
                            {
                                s.Sentiments = _db.Sentiment.Where(t => t.NLPSpeechId == s.Id).ToList();
                                s.NamedEntities = _db.NamedEntity.Where(t => t.NLPSpeechId == s.Id).ToList();
                                s.Segments = _db.SpeechSegment.Where(ss => ss.SpeechId == s.Id).Include(ss => ss.Shouts).ToList();
                                return s;
                            })
                            .ToList();

                        // Generate a json
                        var asJson = JsonConvert.SerializeObject(downloadProtocol);
                        // put it into the zip file
                        var filename = $"LP_{protocol.LegislaturePeriod}_Sitzung_{protocol.Number}.json";
                        var archiveFile = archive.CreateEntry(filename, CompressionLevel.Optimal);
                        // Take the archive file, open it, write in the json, close it
                        using (var entryStream = archiveFile.Open())
                        using (var fileToCompressStream = new MemoryStream(Encoding.UTF8.GetBytes(asJson)))
                        {
                            fileToCompressStream.CopyTo(entryStream);
                        }
                    }
                }

                return outStream.ToArray();
            }
        }

        /// <summary>
        /// Counts the protocols and speeches that fit the filter and returns both counts
        /// </summary>
        /// <returns></returns>
        public (int, int) GetProtocolAndSpeechCountOfFilter(DateTime from,
            DateTime to,
            List<string> fractions,
            List<string> parties,
            List<string> speakerIds)
        {
            var result = (0, 0);

            // Count the protocols
            result.Item1 = _db.Protocols.Where(p => p.Date >= from && p.Date <= to).Count();
            // Count the speeches.
            result.Item2 = _db.Protocols
                .Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.LegislaturePeriod == p.LegislaturePeriod && s.ProtocolNumber == p.Number && _db.Deputies.Any(d => d.SpeakerId == s.SpeakerId)
                    && (
                    speakerIds.Contains(s.SpeakerId)
                    || fractions.Contains(_db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId).Fraction)
                    || parties.Contains(_db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId).Party)
                    )))
                .Count();

            return result;
        }

    }
}
