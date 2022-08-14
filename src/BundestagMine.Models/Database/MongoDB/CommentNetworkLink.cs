using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.Models.Database.MongoDB
{
    public class CommentNetworkLink : DBEntity
    {
        public string Source { get; set; }

        public string Target { get; set; }

        public double Sentiment { get; set; }

        public int Value { get; set; }
    }
}
