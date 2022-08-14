using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BundestagMine.Models.Database.MongoDB
{
    public class CommentNetworkNode : DBEntity
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Party { get; set; }
    }
}
