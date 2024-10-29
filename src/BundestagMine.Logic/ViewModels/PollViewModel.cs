using BundestagMine.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.Logic.ViewModels
{
    public class PollViewModel
    {
        public Poll Poll { get; set; }
        public List<PollEntry> Entries { get; set; }
    }
}
