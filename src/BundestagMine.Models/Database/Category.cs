using System;

namespace BundestagMine.Models.Database
{
    /// <summary>
    /// Represents a topic/category with potnetial subcategories. Exists since the release of VecTop:
    /// https://github.com/TheItCrOw/VecTop
    /// </summary>
    public class Category : DBEntity
    {
        public Category()
        {
            Created = DateTime.Now;
        }

        public string Name { get; set; }
        public string SubCategory { get; set; }

        /// <summary>
        /// The id of the speech this category belongs to.
        /// </summary>
        public Guid NLPSpeechId { get; set; }
        public DateTime Created { get; set; }
    }
}
