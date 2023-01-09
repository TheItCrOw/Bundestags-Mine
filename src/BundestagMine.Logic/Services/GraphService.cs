using BundestagMine.Models.Database.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.Logic.Services
{

    public class GraphService
    {
        private readonly MetadataService _metadataService;
        private readonly BundestagMineDbContext _db;

        public GraphService(BundestagMineDbContext db, MetadataService metadataService)
        {
            _metadataService = metadataService;
            _db = db;
        }
    }
}
