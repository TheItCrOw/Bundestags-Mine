using System;
using System.Collections.Generic;

namespace BundestagMine.ViewModels.Import
{
    public class ImportLogViewModel
    {
        public string FullFilePath { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public Status Status { get; set; }
        public List<string> LogLines { get; set; }
    }

    public enum Status
    {
        Success,
        Warning,
        Error
    }
}
