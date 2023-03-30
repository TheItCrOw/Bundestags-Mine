using BundestagMine.Models.Database.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Logic.Services
{
    public class LaTeXService
    {
        private readonly BundestagScraperService _bundestagScraperService;
        private readonly MetadataService _metadataService;
        private readonly ILogger<LaTeXService> _logger;
        private readonly BundestagMineDbContext _db;

        public LaTeXService(BundestagMineDbContext db,
            ILogger<LaTeXService> logger,
            MetadataService metadataService,
            BundestagScraperService bundestagScraperService)
        {
            _bundestagScraperService = bundestagScraperService;
            _metadataService = metadataService;
            _logger = logger;
            _db = db;
        }

        public string ProtocolToLaTeX(int lp, int number)
        {
            return ProtocolToLaTeX(_db.Protocols.First(p => p.LegislaturePeriod == lp && p.Number == number));
        }

        /// <summary>
        /// Takes in a protocol and builds a latex project from it, which can be parsed to pdf.
        /// This returns the latex code if the protocol
        /// </summary>
        /// <param name="lp"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public string ProtocolToLaTeX(Protocol protocol)
        {
            try
            {
                // we dont check for null references here. If we dont have the config chunk in the database,
                // this wont work anyways. Just go into the exception handler then...
                // we always need the config chunk. No need to change that. 
                var config = _db.LaTeXChunks.FirstOrDefault(c => c.ChunkType == Models.Database.LaTeXChunkType.Config)?.LaTeX;

                // We need the main.tex. This is where we put all the other stuff into.
                var main = _db.LaTeXChunks.FirstOrDefault(c => c.ChunkType == Models.Database.LaTeXChunkType.Main)?.LaTeX;

                // Put the config into the main
                main = main.Replace("**CONFIG**", config);

                // Get the title page, put in the correct params and add them to the main
                var titlePage = BuildLaTeXTitlePage(protocol);
                main = main.Replace("**TITLE-PAGE**", titlePage);

                // Add the prelude
                var prelude = _db.LaTeXChunks.FirstOrDefault(c => c.ChunkType == Models.Database.LaTeXChunkType.Prelude)?.LaTeX;
                main = main.Replace("**PRELUDE**", prelude);

                // Add the speakers list
                main = main.Replace("**SPEAKERS**", BuildLaTeXSpeakersList(_metadataService
                    .GetAllSpeakersOfProtocol(protocol.Number, protocol.LegislaturePeriod)));

                // Now build the actual content, which means the top and speeches etc.
                main = main.Replace("**CONTENT-HERE**", BuildLaTeXContent(protocol));

                return main;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Builds the content meaning top and speeches, comments for given tops
        /// </summary>
        /// <returns></returns>
        private string BuildLaTeXContent(Protocol protocol)
        {
            var latex = new StringBuilder();

            foreach (var agenda in _metadataService.GetAgendaItemsOfProtocol(protocol).ToList())
            {
                // Add the current agendaitem
                var agendaLatex = _db.LaTeXChunks.First(c => c.ChunkType == Models.Database.LaTeXChunkType.AgendaItem).LaTeX;
                agendaLatex = agendaLatex.Replace("**AGENDA-ITEM-NAME**", agenda.Title);
                // Descriptions have hyperlinks to drucksachen. We dont want that in the pdf
                agendaLatex = agendaLatex.Replace("**AGENDA-ITEM-DESCRIPTION**", agenda.Description.StripHTML());
                latex.AppendLine(agendaLatex);
                // We want the speeches to be in 2 cols.
                latex.AppendLine(@"\begin{multicols}{2}");

                // Now add all speeches of that agenda
                foreach (var speech in _metadataService.GetNLPSpeechesOfAgendaItem(
                    protocol.LegislaturePeriod, protocol.Number, agenda.Order))
                {
                    var speaker = _metadataService.GetSpeakerOfSpeech(speech);

                    var speechLatex = _db.LaTeXChunks.First(c => c.ChunkType == Models.Database.LaTeXChunkType.Speech).LaTeX;
                    var speakerName = speaker == null ? "Unbekannt" : speaker.GetFullName();
                    var speakerOrg = speaker == null ? "Plos" : speaker.GetOrga();
                    // add the speaker credentials
                    speechLatex = speechLatex.Replace("**SPEECH-SPEAKER-NAME**", speakerName);
                    speechLatex = speechLatex.Replace("**SPEECH-SPEAKER-ORG**", speakerOrg);
                    // TODO: Add image of speaker?
                    // add the summary
                    speechLatex = speechLatex.Replace("**SPEECH-SUMMARY**", speech.GetAbstractSummary);

                    // Now we build the speech content by getting the segments and adding the comments etc.
                    var speechTextLatex = new StringBuilder();
                    foreach (var segment in _db.SpeechSegment.Where(ss => ss.SpeechId == speech.Id))
                    {
                        speechTextLatex.AppendLine(segment.Text.StripTabs());
                        // Are there any shouts? If so, add them!
                        foreach (var shout in _db.Shouts.Where(sh => sh.SpeechSegmentId == segment.Id))
                        {
                            var shouterName = string.IsNullOrEmpty(shout.SpeakerId)
                                ? ""
                                : shout.FirstName + " " + shout.LastName;
                            var shouterOrg = string.IsNullOrEmpty(shout.SpeakerId)
                                ? "/"
                                : shout.Fraction;
                            var commentLatex = _db.LaTeXChunks.First(c => c.ChunkType == Models.Database.LaTeXChunkType.Comment).LaTeX;
                            commentLatex = commentLatex.Replace("**COMMENTATOR-NAME**", shouterName);
                            commentLatex = commentLatex.Replace("**COMMENTATOR-ORG**", shouterOrg);
                            commentLatex = commentLatex.Replace("**COMMENT-TEXT**", shout.Text.StripTabs());
                            // Image of the potential shouter
                            var imgPath = _bundestagScraperService.GetDeputyPortraitFilePath(shout.SpeakerId);
                            if (File.Exists(imgPath))
                                commentLatex = commentLatex.Replace("**IMAGE-PATH**", imgPath.Replace(@"\", "/"));
                            else
                                commentLatex = commentLatex.Replace("**IMAGE-PATH**",
                                    ConfigManager.GetUnknownImage().Replace(@"\", "/"));

                            speechTextLatex.AppendLine(commentLatex);
                        }
                    }
                    speechLatex = speechLatex.Replace("**SPEECH-TEXT**", speechTextLatex.ToString());
                    latex.AppendLine(speechLatex.ToString());
                }
                // End the multicols and start a new page
                latex.AppendLine(@"\end{multicols}");
                latex.AppendLine(@"\newpage"); // After an top ends, we want to start a new page.
            }

            return latex.ToString();
        }

        /// <summary>
        /// Builds the latex list of all speakers by a given list of deputies
        /// </summary>
        /// <param name="deputies"></param>
        /// <returns></returns>
        private string BuildLaTeXSpeakersList(List<Deputy?> deputies)
        {
            var speakerListLaTeX = _db.LaTeXChunks.FirstOrDefault(c => c.ChunkType == Models.Database.LaTeXChunkType.SpeakerList)?.LaTeX;
            var latex = new StringBuilder();
            foreach (var deputy in deputies)
            {
                if (deputy == null) continue;
                latex.AppendLine($"{deputy.GetFullName()} ({deputy.GetOrga()})");
                latex.AppendLine(@"\newline");
            }
            return speakerListLaTeX.Replace("**SPEAKERS-LIST**", latex.ToString());
        }

        /// <summary>
        /// Builds the title page for the given protocol
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        private string BuildLaTeXTitlePage(Protocol protocol)
        {
            var titlePage = _db.LaTeXChunks.FirstOrDefault(c => c.ChunkType == Models.Database.LaTeXChunkType.TitlePage)?.LaTeX;
            titlePage = titlePage.Replace("**DATE**", protocol.Date.ToShortDateString());
            titlePage = titlePage.Replace("**LP**", protocol.LegislaturePeriod.ToString());
            titlePage = titlePage.Replace("**PROTOCOL-NUMBER**", protocol.Number.ToString());
            titlePage = titlePage.Replace("**LOGO**", ConfigManager.GetProtocolLaTeXLogo().Replace(@"\", "/"));
            return titlePage;
        }

        /// <summary>
        /// Converts a latex project (files) to a pdf by executing the pdfLatex command on the cmd.
        /// For this to work the os has to install MiKTeX: https://miktex.org/
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public string LaTeXToPDF(string latex)
        {
            // We write the latex code to disc for now and then call the cmd to compile it.
            var baseDir = ConfigManager.GetProtocolWorkingDirectoryPath();
            var pdfId = Guid.NewGuid();
            // This is the dir we copy the complete tex code in and then build the pdf from
            var workingDir = Path.Combine(baseDir, pdfId.ToString());
            // Create it and write
            Directory.CreateDirectory(workingDir);
            File.WriteAllText(Path.Combine(workingDir, "main.tex"), latex);

            try
            {
                var startInfo = new ProcessStartInfo(@"C:\Windows\System32\cmd.exe")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    Arguments = "/c " + ConfigManager.GetBuildPDFCommand(),
                    WorkingDirectory = workingDir
                };

                string StartConverting()
                {
                    // Start the script
                    using (var process = Process.Start(startInfo))
                    {
                        using (var reader = process?.StandardOutput)
                        {
                            // Wait until its finished
                            var result = reader?.ReadToEnd();
                            return "GOOD";
                        }
                    }
                }

                // Alright, what the hell is going on here? LaTeX is full of suprises. So, appearntly the 
                // table of contents cannot be calculted the first time the tex file is being compiled. The 
                // solution: Let it compile twice. Thats not a hack, thats how it is... So we run it once and
                // then again!
                var firstRun = StartConverting();
                var secondRun = StartConverting();
                return Path.Combine(workingDir, @"out\main.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown error when trying to generate the pdf from latex: ");
                return null;
            }
        }
    }
}
