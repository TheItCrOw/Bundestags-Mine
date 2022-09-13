using BundestagMine.Models.Database.MongoDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.Synchronisation.Services
{
    public static class CacheService
    {
        public static List<Shout> ImportedShouts { get; set; } = new List<Shout>();
        public static int NewProtocolsStored { get; set; } = 0;
    }
}
