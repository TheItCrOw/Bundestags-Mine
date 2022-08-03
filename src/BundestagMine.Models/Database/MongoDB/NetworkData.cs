using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace BundestagMine.Models.Database.MongoDB
{
    /// <summary>
    /// Sadly, we cannot deserilize the networkdata because the "id" field is always null. Probably because the _id
    /// of Mongo gets interfered with it... Dont know. Deserialize the networkdata by hand I guess.
    /// </summary>
    public class NetworkData : MongoDBEntity
    {
        [BsonElement("nodes")]
        public List<CommentNetworkNode> Nodes { get; set; }

        [BsonElement("links")]
        public List<CommentNetworkLink> Links { get; set; }
    }
}
