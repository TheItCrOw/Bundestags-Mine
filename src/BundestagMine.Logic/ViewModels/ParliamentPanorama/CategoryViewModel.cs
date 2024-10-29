using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Logic.ViewModels.ParliamentPanorama
{
    public class CategoryViewModel
    {
        public Guid NLPSpeechId { get; set; }
        public string Name { get; set; }
        public List<string> SubCategories { get; set; }
    }
}
