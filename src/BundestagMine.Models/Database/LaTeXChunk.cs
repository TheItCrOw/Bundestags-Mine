using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Models.Database
{
    /// <summary>
    /// Each LaTeX Chunk is a template chunk we built in overleaf. We use these chunks to build
    /// the protocol pdfs export.
    /// </summary>
    public class LaTeXChunk : DBEntity
    {
        public LaTeXChunkType ChunkType { get; set; }
        public string LaTeX { get; set; }
    }

    public enum LaTeXChunkType
    {
        Config,
        Main,
        TitlePage,
        Prelude, // Vorwort
        SpeakerList,
        AgendaItem,
        Speech,
        Comment,
        EvaluationBox,
        SummarySchuerfer,
        SpeechSegment,
        Poll
    }
}
