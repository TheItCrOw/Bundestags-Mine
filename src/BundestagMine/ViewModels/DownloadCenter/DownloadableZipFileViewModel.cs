using System;

namespace BundestagMine.ViewModels.DownloadCenter
{
    public class DownloadableZipFileViewModel
    {
        public string FileName { get; set; }
        public double SizeInMb { get; set; }
        public DateTime Created { get; set; }

        /// <summary>
        /// The time when the zip file will be deleted automatically.
        /// </summary>
        public DateTime DeletionTime { get; set; }
    }
}
