using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BundestagMine.Models.Database.MongoDB
{
    /// <summary>
    /// Sadly, we cannot deserilize the node because the "id" field is always null. Probably because the _id
    /// of Mongo gets interfered with it... Dont know. Deserialize the networkdata by hand I guess.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class CommentNetworkNode : DBEntity
    {
        [BsonElement("id")]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("party")]
        public string Party { get; set; }
    }
}
