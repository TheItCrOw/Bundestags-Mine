using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.Models.Database.MongoDB
{
    public class CommentNetworkLink : DBEntity
    {
        [BsonElement("source")]
        public string Source { get; set; }

        [BsonElement("target")]
        public string Target { get; set; }

        [BsonElement("sentiment")]
        public double Sentiment { get; set; }

        [BsonElement("value")]
        public int Value { get; set; }
    }
}
