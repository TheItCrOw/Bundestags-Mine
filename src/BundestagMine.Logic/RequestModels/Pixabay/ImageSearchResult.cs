using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Logic.RequestModels.Pixabay
{
    public class ImageSearchResult
    {

        /// <summary>
        /// This is how many image were found
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// This is how many images were actually returned
        /// </summary>
        public int TotalHits { get; set; }

        public List<SearchHit> Hits { get; set; }
    }
}
