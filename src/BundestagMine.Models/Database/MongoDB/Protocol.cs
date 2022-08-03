using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.Models.Database.MongoDB
{
    public class Protocol : MongoDBEntity
    {
        [BsonElement("date")]
        public DateTime Date { get; set; }

        [BsonElement("legislaturePeriod")]
        public int LegislaturePeriod { get; set; }

        [BsonElement("number")]
        public int Number { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("agendaItemsCount")]
        public int AgendaItemsCount { get; set; }
    }
}
