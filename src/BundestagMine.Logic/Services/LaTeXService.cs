using BundestagMine.Logic.HelperModels;
using BundestagMine.Logic.ViewModels.DailyPaper;
using BundestagMine.Models.Database;
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
        private readonly DailyPaperService _dailyPaperService;
        private readonly AnnotationService _annotationService;
        private readonly BundestagScraperService _bundestagScraperService;
        private readonly MetadataService _metadataService;
        private readonly ILogger<LaTeXService> _logger;
        private readonly BundestagMineDbContext _db;

        public LaTeXService(BundestagMineDbContext db,
            ILogger<LaTeXService> logger,
            MetadataService metadataService,
            BundestagScraperService bundestagScraperService,
            AnnotationService annotationService,
            DailyPaperService dailyPaperService)
        {
            _dailyPaperService = dailyPaperService;
            _annotationService = annotationService;
            _bundestagScraperService = bundestagScraperService;
            _metadataService = metadataService;
            _logger = logger;
            _db = db;
        }

        public LaTeXProtocol ProtocolToLaTeX(int lp, int number)
        {
            return ProtocolToLaTeX(_db.Protocols.FirstOrDefault(p => p.LegislaturePeriod == lp && p.Number == number));
        }

        /// <summary>
        /// Takes in a protocol and builds a latex project from it, which can be parsed to pdf.
        /// This returns the latex code if the protocol
        /// </summary>
        /// <param name="lp"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public LaTeXProtocol ProtocolToLaTeX(Protocol protocol)
        {
            try
            {
                if (protocol == null) return null;

                var latexProtocol = new LaTeXProtocol() { Id = Guid.NewGuid() };
                // We need a tmp folder to store images and such
                latexProtocol.TmpFolderPath = Path.Combine(ConfigManager.GetLaTeXTmpsPath(), latexProtocol.Id.ToString());
                Directory.CreateDirectory(latexProtocol.TmpFolderPath);

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

                // Add the schuerfer summary here
                main = main.Replace("**SUMMARY-SCHUERFER**", BuildSchuerferSummary(protocol));

                // Now build the actual content, which means the top and speeches etc.
                main = main.Replace("**CONTENT-HERE**", BuildLaTeXContent(protocol));

                latexProtocol.LaTeX = main;
                return latexProtocol;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error creating the latex for a protocol!");
                return null;
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
                agendaLatex = agendaLatex.Replace("**AGENDA-ITEM-NAME**", agenda.Title.Replace("\"", "``"));
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
                            var commentLatex = BuildCommentChunkFromShout(shout);
                            speechTextLatex.AppendLine(commentLatex);
                        }
                    }
                    speechLatex = speechLatex.Replace("**SPEECH-TEXT**", speechTextLatex.ToString());
                    // Also add the evaluation box
                    speechLatex = speechLatex.Replace("**EVALUATION-BOX**", BuildSpeechEvaluationBox(speech));
                    latex.AppendLine(speechLatex.ToString());
                }
                // End the multicols and start a new page
                latex.AppendLine(@"\end{multicols}");
                latex.AppendLine(@"\newpage"); // After an top ends, we want to start a new page.
            }

            return latex.ToString();
        }

        /// <summary>
        /// Builds the comment chunk from a shout
        /// </summary>
        /// <param name="shout"></param>
        /// <returns></returns>
        private string BuildCommentChunkFromShout(Shout shout)
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
            // Image of the potential shouter.
            // Update: I dont think I want shouter images. So cut them out for now
            /*
            var imgPath = _bundestagScraperService.GetDeputyPortraitFilePath(shout.SpeakerId);
            if (File.Exists(imgPath))
                commentLatex = commentLatex.Replace("**IMAGE-PATH**", imgPath.Replace(@"\", "/"));
            else
                commentLatex = commentLatex.Replace("**IMAGE-PATH**",
                    ConfigManager.GetUnknownImage().Replace(@"\", "/"));
            */

            return commentLatex;
        }

        /// <summary>
        /// We build a short summary of the protocol by using the dailypapers of the schuerfer
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        private string BuildSchuerferSummary(Protocol protocol)
        {
            var schuerferLaTeX = _db.LaTeXChunks.FirstOrDefault(c => c.ChunkType == Models.Database.LaTeXChunkType.SummarySchuerfer)?.LaTeX;
            var dailyPaper = _dailyPaperService.GetDailyPaperAsViewModel(protocol.LegislaturePeriod, protocol.Number);
            if (dailyPaper == default) return "Keine Übersicht vom Schürfer vorhanden.";
            // NE stuff
            schuerferLaTeX = schuerferLaTeX.Replace("**TOPIC**", dailyPaper.FirstSpecialTopicOfTheDay);
            schuerferLaTeX = schuerferLaTeX.Replace("**TOPIC-1**", dailyPaper.FirstSpecialTopicOfTheDay);
            schuerferLaTeX = schuerferLaTeX.Replace("**TOPIC-2**", dailyPaper.SecondSpecialTopicOfTheDay);
            schuerferLaTeX = schuerferLaTeX.Replace("**TOPIC-3**", dailyPaper.ThirdSpecialTopicOfTheDay);

            // Now the speech segments
            // Most commented
            schuerferLaTeX = schuerferLaTeX.Replace("**SEGMENT-MOST-COMMENTED**", 
                BuildSpeechSegment(dailyPaper.MostCommentedSpeech, "Meistkommentierte Rede"));
            // Most positive
            schuerferLaTeX = schuerferLaTeX.Replace("**SEGMENT-MOST-POSITIVE**",
                BuildSpeechSegment(dailyPaper.MostPositiveSpeech, @"\faCogs \hspace{1mm} Positivste Rede"));
            // Most negative
            schuerferLaTeX = schuerferLaTeX.Replace("**SEGMENT-MOST-NEGATIVE**",
                BuildSpeechSegment(dailyPaper.MostNegativeSpeech, @"\faCogs \hspace{1mm} Negativste Rede"));

            // Polls
            var pollsLatex = new StringBuilder();
            foreach (var poll in _metadataService.GetPollsOfProtocol(protocol))
            {
                pollsLatex.AppendLine(BuildPoll(poll));
                //pollsLatex.AppendLine(@"\newline");
                //pollsLatex.AppendLine(@"\newline");
            }
            if (pollsLatex.Length == 0) pollsLatex.AppendLine("Keine Abstimmungen in dieser Sitzung.");
            schuerferLaTeX = schuerferLaTeX.Replace("**POLLS**", pollsLatex.ToString());

            return schuerferLaTeX;
        }

        /// <summary>
        /// Builds the poll latex for a given poll
        /// </summary>
        /// <param name="poll"></param>
        /// <returns></returns>
        private string BuildPoll(Poll poll)
        {
            var pollLatex = _db.LaTeXChunks.FirstOrDefault(c => c.ChunkType == Models.Database.LaTeXChunkType.Poll)?.LaTeX;

            var totalVotes = poll.Entries.Count;
            var yes = 100 / (double)totalVotes * (double)poll.Entries.Where(e => e.Yes).Count();
            var no = 100 / (double)totalVotes * (double)poll.Entries.Where(e => e.No).Count();
            var abstention = 100 / (double)totalVotes * (double)poll.Entries.Where(e => e.Abstention).Count();
            var rest = 100 / (double)totalVotes * (double)poll.Entries.Where(e => !e.Yes && !e.No && !e.Abstention).Count();

            pollLatex = pollLatex.Replace("**YES**", yes.ToString("F").Replace(",", "."));
            pollLatex = pollLatex.Replace("**NO**", no.ToString("F").Replace(",", "."));
            pollLatex = pollLatex.Replace("**ENTHALTEN**", abstention.ToString("F").Replace(",", "."));
            pollLatex = pollLatex.Replace("**OTHER**", rest.ToString("F").Replace(",", "."));
            pollLatex = pollLatex.Replace("**POLL-TITLE**", poll.Title);

            return pollLatex;
        }

        /// <summary>
        /// Builds a speech segment chunk from a SpeechPartViewModel of the dailypaper
        /// </summary>
        /// <param name="speechPartViewModel"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private string BuildSpeechSegment(SpeechPartViewModel speechPartViewModel, string title)
        {
            var segmentLatex = _db.LaTeXChunks.FirstOrDefault(c => c.ChunkType == Models.Database.LaTeXChunkType.SpeechSegment)?.LaTeX;
            segmentLatex = segmentLatex.Replace("**TITLE**", title);
            segmentLatex = segmentLatex.Replace("**SPEAKER-NAME**", speechPartViewModel.Speaker?.GetFullName());
            segmentLatex = segmentLatex.Replace("**SPEAKER-ORG**", speechPartViewModel.Speaker?.GetOrga());
            segmentLatex = segmentLatex.Replace("**TOP**", speechPartViewModel.AgendaItem?.Title);
            var nlp = _db.NLPSpeeches.FirstOrDefault(s => s.Id == speechPartViewModel.Speech.Id);
            segmentLatex = segmentLatex.Replace("**SPEECH-SUMMARY**", nlp.GetAbstractSummary);

            // We show one segment and their comments
            var speechSegmentText = new StringBuilder();
            var segment = speechPartViewModel.Speech.Segments.OrderByDescending(ss => ss.Shouts.Where(sh => !sh.Text.ToLower().Contains("befaill")).Count()).First();
            speechSegmentText.AppendLine(segment.Text);
            foreach (var shout in segment.Shouts)
            {
                var commentLatex = BuildCommentChunkFromShout(shout);
                speechSegmentText.AppendLine(commentLatex);
            }
            segmentLatex = segmentLatex.Replace("**SPEECH-SEGMENT**", speechSegmentText.ToString());
            // Get the eval and add it
            segmentLatex = segmentLatex.Replace("**SPEECH-EVALUATION**", BuildSpeechEvaluationBox(nlp));

            return segmentLatex;
        }

        /// <summary>
        /// Builds the evaluation box latex code
        /// </summary>
        /// <returns></returns>
        private string BuildSpeechEvaluationBox(Speech speech)
        {
            var speechEvaluationBox = _db.LaTeXChunks.FirstOrDefault(c => c.ChunkType == Models.Database.LaTeXChunkType.EvaluationBox)?.LaTeX;

            // Get the top 3 named entities
            var nes = _annotationService.GetMostUsedNamedEntitiesOfSpeech(speech, 3);
            speechEvaluationBox = speechEvaluationBox.Replace("**NE-1**", nes.Count > 0 ? nes[0] : "/");
            speechEvaluationBox = speechEvaluationBox.Replace("**NE-2**", nes.Count > 1 ? nes[1] : "/");
            speechEvaluationBox = speechEvaluationBox.Replace("**NE-3**", nes.Count > 2 ? nes[2] : "/");

            // comments count
            speechEvaluationBox = speechEvaluationBox.Replace("**COMMENTS-COUNT**",
                _metadataService.GetActualCommentsAmountOfSpeech(speech).ToString());

            // applause count
            speechEvaluationBox = speechEvaluationBox.Replace("**APPLAUSE-COUNT**",
                _metadataService.GetApplaudCommentsAmountOfSpeech(speech).ToString());

            // avg sentiment
            var avg = _annotationService.GetAverageSentimentOfSpeech(speech);
            var avgLaTex = @"\textcolor{**COLOR**}{" + avg.ToString("F") + "}";
            if (avg > 0) avgLaTex = avgLaTex.Replace("**COLOR**", "light-green");
            else if (avg == 0) avgLaTex = avgLaTex.Replace("**COLOR**", "light-blue");
            else if (avg < 0) avgLaTex = avgLaTex.Replace("**COLOR**", "light-red");
            speechEvaluationBox = speechEvaluationBox.Replace("**AVG-SENTIMENT**", avgLaTex);

            // Draw the pie chart
            var sentiments = _annotationService.GetSentimentsForGraphs(DateTime.Now, DateTime.Now, "", "", "",
                speech.Id.ToString());
            var pos = sentiments.FirstOrDefault(s => s.Value == "pos")?.Count;
            var neg = sentiments.FirstOrDefault(s => s.Value == "neg")?.Count;
            var neu = sentiments.FirstOrDefault(s => s.Value == "neu")?.Count;
            // for the pie chart, we need percentages
            var total = (pos == null ? 0 : pos) + (neg == null ? 0 : neg) + (neu == null ? 0 : neu);
            speechEvaluationBox = speechEvaluationBox.Replace("**POS**", pos == null ? "0"
                : (100.0 / (double)total * (double)pos).ToString("F").Replace(",", ".")); // Latex works with . not with ,
            speechEvaluationBox = speechEvaluationBox.Replace("**NEG**", neg == null ? "0"
                : (100.0 / (double)total * (double)neg).ToString("F").Replace(",", "."));
            speechEvaluationBox = speechEvaluationBox.Replace("**NEU**", neu == null ? "0"
                : (100.0 / (double)total * (double)neu).ToString("F").Replace(",", "."));

            return speechEvaluationBox;
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
        public string LaTeXToPDF(LaTeXProtocol latexProtocol)
        {
            // We write the latex code to disc for now and then call the cmd to compile it.
            var baseDir = ConfigManager.GetProtocolWorkingDirectoryPath();
            // This is the dir we copy the complete tex code in and then build the pdf from
            var workingDir = Path.Combine(baseDir, latexProtocol.Id.ToString());
            // Create it and write
            Directory.CreateDirectory(workingDir);
            File.WriteAllText(Path.Combine(workingDir, "main.tex"), latexProtocol.LaTeX);

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
