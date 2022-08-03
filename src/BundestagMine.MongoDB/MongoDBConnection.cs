using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.MongoDB
{
    public class MongoDBConnection
    {
        /// <summary>
        /// Tries to connect to the MongoDB from Atlas and rerturns the <see cref="IMongoDatabase"/>
        /// </summary>
        /// <returns></returns>
        public static bool Connect(out IMongoDatabase db, string dbName = "bundestagMining")
        {
            try
            {
                var settings = MongoClientSettings.FromConnectionString("mongodb+srv://TheItCrOW:w{KbL*GmWJ}a=N6iykUyGi;{L0I!s7j^@cluster0.bjqsv.mongodb.net/bundestagMining?retryWrites=true&w=majority");
                settings.ServerApi = new ServerApi(ServerApiVersion.V1);
                var client = new MongoClient(settings);
                db = client.GetDatabase(dbName);

                return true;
            }
            catch(Exception ex)
            {
                db = null;
                Console.WriteLine("Couldn't connect to the mongoDB database.");
                Console.WriteLine(ex);
                return false;
            }
        }
    }
}
