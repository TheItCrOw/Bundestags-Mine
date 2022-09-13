using BundestagMine.Models.Database.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.Services
{
    public class TopicMapGraphObject
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public List<TopicMapGraphObject> Children { get; set; }

        /// <summary>
        /// Some nodes have speakerIds
        /// </summary>
        public string SpeakerId { get; set; }

        /// <summary>
        /// We only use this in the speaker bubble
        /// </summary>
        public string NamedEntityLemmaValue { get; set; }
    }

    public class TopicBarRaceGraphObject
    {
        public string Name { get; set; }
        public int Value { get; set; }

        /// <summary>
        /// Its a string because we have subyear steps like 2000, 2000.1, 20002, ...
        /// </summary>
        public string Year { get; set; }
        public int LastValue { get; set; }
        public int Rank { get; set; }
    }

    public class GraphService
    {
        private readonly MetadataService _metadataService;
        private readonly BundestagMineDbContext _db;

        public GraphService(BundestagMineDbContext db, MetadataService metadataService)
        {
            _metadataService = metadataService;
            _db = db;
        }

        /// <summary>
        /// Builds the csv data needed for the topic bar race
        /// </summary>
        /// <returns></returns>
        public List<TopicBarRaceGraphObject> BuildTopicBarRaceChartData()
        {
            var result = new List<TopicBarRaceGraphObject>();
            // We build the race from 2017 to today
            var from = DateTime.Parse("01.01.2017");
            var to = DateTime.Now;

            // This stores the last value of a topic
            var topicToLastValue = new Dictionary<string, int>();

            // We step through the years in monthly steps.
            for (; from < to; from = from.AddMonths(1))
            {
                var curTo = from.AddMonths(1).AddDays(-1);
                var topics = _db.Protocols.Where(p => p.Date >= from && p.Date <= curTo)
                    .SelectMany(p => _db.Speeches
                        .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod))
                    .SelectMany(s => _db.NamedEntity.Where(t => s.Id == t.NLPSpeechId && !TopicHelper.TopicBlackList.Contains(t.LemmaValue)))
                    .AsEnumerable()
                    .GroupBy(n => n.LemmaValue)
                    .Select(g1 =>
                    {
                        var graphObject = new TopicBarRaceGraphObject()
                        {
                            Value = g1.Count(),
                            Year = $"{from.Year}.{from.Month}",
                            Name = g1.Key,
                        };

                        // Check if there is a last value
                        if (topicToLastValue.TryGetValue(g1.Key, out var value))
                        {
                            graphObject.LastValue = value;
                            topicToLastValue[g1.Key] = graphObject.Value;
                            graphObject.Value += value;
                        }
                        else
                            topicToLastValue[g1.Key] = graphObject.Value;

                        return graphObject;
                    })
                    .OrderByDescending(go => go.Value)
                    .Take(20)
                    .ToList();

                for (int i = 0; i < topics.Count; i++)
                {
                    var topic = topics[i];
                    topic.Rank = i + 1;
                }

                result.AddRange(topics);
            }

            return result;
        }


        /// <summary>
        /// Gets the json data for the topic map 
        /// </summary>
        /// <returns></returns>
        public TopicMapGraphObject BuildTopicMapData(DateTime from, DateTime to)
        {
            // Root: Bundestag
            // Children 1: Fractions
            // Children 2: Top 20 NE
            // Children 3 speeches?

            // id = fraction name
            var fractions = _metadataService.GetFractions();
            var root = new TopicMapGraphObject()
            {
                Name = "Bundestag",
                Children = new List<TopicMapGraphObject>()
            };

            // Add the fraction nodes
            foreach (var fraction in fractions)
            {
                var graphFraction = new TopicMapGraphObject()
                {
                    Name = fraction.id
                };

                graphFraction.Children = _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                    .SelectMany(p => _db.Speeches
                        .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod &&
                            _db.Deputies.SingleOrDefault(d => d.SpeakerId == s.SpeakerId).Fraction == graphFraction.Name))
                    .SelectMany(s => _db.NamedEntity.Where(n => s.Id == n.NLPSpeechId && n.LemmaValue != null 
                                && !TopicHelper.TopicBlackList.Contains(n.LemmaValue)))
                    .AsEnumerable()
                    .GroupBy(n => n.LemmaValue.ToLower())
                    .Select(g1 => new TopicMapGraphObject()
                    {
                        Name = g1.Key,
                        Value = g1.Count(),
                    })
                    .OrderByDescending(n => n.Value)
                    .Take(30)
                    .ToList();

                // How often does each deputy of the given fraction use the entity in the given timeframe?
                foreach (var entityChild in graphFraction.Children)
                {
                    var deputyToEntityCount = new Dictionary<Deputy, int>();
                    foreach (var deputy in _db.Deputies.Where(d => d.Fraction == graphFraction.Name).ToList())
                    {
                        var count = _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                                       .SelectMany(p => _db.Speeches
                                           .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                                            && s.SpeakerId == deputy.SpeakerId))
                                       .SelectMany(s => _db.NamedEntity.Where(n => n.LemmaValue == entityChild.Name && n.NLPSpeechId == s.Id))
                                       .Count();
                        deputyToEntityCount[deputy] = count;
                    }

                    entityChild.Children = deputyToEntityCount
                        .OrderByDescending(kv => kv.Value)
                        .Take(10)
                        .Select(kv => new TopicMapGraphObject()
                        {
                            Name = kv.Key.FirstName + " " + kv.Key.LastName,
                            Value = kv.Value,
                            SpeakerId = kv.Key.SpeakerId,
                            NamedEntityLemmaValue = entityChild.Name
                        })
                        .ToList();
                }
                Debug.WriteLine($"Fraktion {graphFraction.Name} finished.");
                if (graphFraction.Children.Count > 0)
                    root.Children.Add(graphFraction);
            }

            return root;
        }
    }
}
