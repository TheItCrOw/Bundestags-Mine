using BundestagMine.Models.Database.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.ViewModels
{
    public class SpeechCommentViewModel
    {
        public Deputy Speaker { get; set; }
        public Guid SpeechId { get; set; }
        public SpeechSegment SpeechSegment { get; set; }
    }
}
