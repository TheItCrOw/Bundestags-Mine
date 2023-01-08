using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Logic.RequestModels.Pixabay
{
    public class SearchHit
    {
        public int Id { get; set; }

        public string PageUrl { get; set; }

        public string LargeImageUrl { get; set; }
        
        public string User { get; set; }

        public int Views { get; set; }
    }
}
