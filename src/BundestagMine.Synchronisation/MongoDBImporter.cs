using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MongoDB.Bson;
using System.Threading.Tasks;
using BundestagMine.SqlDatabase;

namespace BundestagMine.Synchronisation
{
    public class MongoDBImporter
    {
        private readonly IMongoDatabase _db;

        public MongoDBImporter(IMongoDatabase db)
        {
            _db = db;
        }

        /// <summary>
        /// Imports json files from disc to the MongoDB
        /// </summary>
        public void Import(string type)
        {
            Console.WriteLine($"Importing {type}");
            var collection = _db.GetCollection<BsonDocument>(type.ToLower());
            var i = 0;

            foreach (string file in Directory.EnumerateFiles(ConfigManager.GetLocalDataDirectory() + type, "*.json"))
            {
                var curJsonContent = File.ReadAllText(file);
                var document = BsonDocument.Parse(curJsonContent);
                collection.InsertOne(document);
                Console.WriteLine($"Inserted {i}");
                i++;
            }
        }
    }
}
