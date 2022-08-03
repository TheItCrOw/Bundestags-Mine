using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.Models.Database.MongoDB
{
    [BsonIgnoreExtraElements]
    public class Speech : MongoDBEntity
    {
        public string Text { get; set; }
        public string SpeakerId { get; set; }
        public List<SpeechSegment> Segments { get; set; }
        public int ProtocolNumber { get; set; }
        public int LegislaturePeriod { get; set; }
        public int AgendaItemNumber { get; set; }
    }
}
