using BundestagMine.Models.Database.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.Utility;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace BundestagMine.Synchronisation.Services
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
        private List<dynamic> GetFractions(BundestagMineDbContext db)
        {
            var fractions = new List<dynamic>();
            foreach (var deputy in db.Deputies.ToList())
            {
                if (!string.IsNullOrEmpty(deputy.Fraction) && !fractions.Any(p => p.id == deputy.Fraction))
                {
                    dynamic fraction = new ExpandoObject();
                    fraction.id = deputy.Fraction;
                    fractions.Add(fraction);
                }
            }
            return fractions;
        }

        /// <summary>
        /// Builds the data model for the comment network.
        /// </summary>
        /// <returns></returns>
        public void BuildAndStoreCommentNetworkData()
        {
            try
            {
                // This is optimal to save after every add, but my patience is running thin and I had to many exceptions...
                using (var db = new BundestagMineDbContext(ConfigManager.GetDbOptions()))
                {
                    NetworkData networkData = db.NetworkDatas.FirstOrDefault();
                    if (networkData == default)
                    {
                        networkData = new NetworkData()
                        {
                            Id = Guid.NewGuid()
                        };
                        db.NetworkDatas.Add(networkData);
                        db.SaveChanges();
                    }

                    foreach (var shout in CacheService.ImportedShouts.Where(s => !string.IsNullOrEmpty(s.SpeakerId)))
                    {
                        var shouter = db.Deputies.FirstOrDefault(d => d.SpeakerId == shout.SpeakerId);
                        // Speaker of the current speech the shouter is commenting in.
                        var speaker = db.SpeechSegment
                            .Where(ss => ss.Id == shout.SpeechSegmentId)
                            .SelectMany(ss => db.Speeches.Where(s => s.Id == ss.SpeechId))
                            .SelectMany(s => db.Deputies.Where(d => d.SpeakerId == s.SpeakerId))?
                            .First();

                        // If the shouter doesnt already have a node, create it
                        if (!db.CommentNetworkNode.Any(n => n.Id == shouter.SpeakerId))
                        {
                            db.CommentNetworkNode.Add(new CommentNetworkNode()
                            {
                                Id = shouter.SpeakerId,
                                Name = shouter.FirstName + " " + shouter.LastName,
                                Party = shouter.Fraction ?? shouter.Party,
                                NetworkDataId = networkData.Id
                            });
                            db.SaveChanges();
                        }

                        // If the speaker doesnt already have a node, create it. Same thing with the shouter
                        if (!db.CommentNetworkNode.Any(n => n.Id == speaker.SpeakerId))
                        {
                            db.CommentNetworkNode.Add(new CommentNetworkNode()
                            {
                                Id = speaker.SpeakerId,
                                Name = speaker.FirstName + " " + speaker.LastName,
                                Party = speaker.Fraction ?? speaker.Party,
                                NetworkDataId = networkData.Id
                            });
                            db.SaveChanges();
                        }

                        // Now create or update the link of these two.
                        var existingLink = db.CommentNetworkLink.FirstOrDefault(l => l.Source == shouter.SpeakerId && l.Target == speaker.SpeakerId);
                        if (existingLink != default)
                        {
                            existingLink.Value++;
                            // Not sure if I have to do this or if it updates...
                            db.CommentNetworkLink.Update(existingLink);
                            db.SaveChanges();
                        }
                        else
                        {
                            // Ill need to redo that and then come back here.
                            // => Added it into the import. Maybe we have to reimport everything from scratch or I just fix the old.
                            db.CommentNetworkLink.Add(new CommentNetworkLink()
                            {
                                Sentiment = db.Sentiment.FirstOrDefault(s => s.ShoutId == shout.Id)?.SentimentSingleScore ?? 0.0,
                                Source = shouter.SpeakerId,
                                Target = speaker.SpeakerId,
                                Value = 1,
                                NetworkDataId = networkData.Id
                            });
                            db.SaveChanges();
                        }
                    }

                    Log.Information("Build the data - trying to store it...");
                    db.SaveChanges();
                    Log.Information("Stored it!");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while trying to build and store the comment network data: ");
            }
        }

        /// <summary>
        /// Builds the csv data needed for the topic bar race
        /// </summary>
        /// <returns></returns>
        public List<TopicBarRaceGraphObject> BuildTopicBarRaceChartData()
        {
            try
            {
                Log.Information("Recalculating the Topic Bar Race Chart...");
                var result = new List<TopicBarRaceGraphObject>();
                // We build the race from 2017 to today
                var from = DateTime.Parse("01.01.2017");
                var to = DateTime.Now;

                // This stores the last value of a topic
                var topicToLastValue = new Dictionary<string, int>();

                // Some ne are just not good showing in a bar race or are redundant. We blacklist them so they dont
                // take away space.
                var neBlackList = new List<string>()
                {
                    "deutsch", "Präsident !", "europäisch", "europäische", "Zeit", "lieben Kollegin"
                };

                using (var db = new BundestagMineDbContext(ConfigManager.GetDbOptions()))
                {
                    // We step through the years in monthly steps.
                    for (; from < to; from = from.AddMonths(1))
                    {
                        Log.Information($"Currently doing {from.ToShortDateString()}");

                        var curTo = from.AddMonths(1).AddDays(-1);
                        var topics = db.Protocols.Where(p => p.Date >= from && p.Date <= curTo)
                            .SelectMany(p => db.Speeches
                                .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod))
                            .SelectMany(s => db.NamedEntity.Where(t => s.Id == t.NLPSpeechId && !neBlackList.Contains(t.LemmaValue)))
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
                }

                Log.Information("Done! Topic Bar Race Chart calculated.");
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while trying to recalculate the topic bar race chart: ");
                return null;
            }
        }


        /// <summary>
        /// Gets the json data for the topic map 
        /// </summary>
        /// <returns></returns>
        public TopicMapGraphObject BuildTopicMapData(DateTime from, DateTime to)
        {
            try
            {
                // Root: Bundestag
                // Children 1: Fractions
                // Children 2: Top 20 NE
                // Children 3 speeches?
                using (var db = new BundestagMineDbContext(ConfigManager.GetDbOptions()))
                {
                    // id = fraction name
                    var fractions = GetFractions(db);
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

                        graphFraction.Children = db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                            .SelectMany(p => db.Speeches
                                .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod &&
                                    db.Deputies.SingleOrDefault(d => d.SpeakerId == s.SpeakerId).Fraction == graphFraction.Name))
                            .SelectMany(s => db.NamedEntity.Where(n => s.Id == n.NLPSpeechId && n.LemmaValue != null))
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
                            foreach (var deputy in db.Deputies.Where(d => d.Fraction == graphFraction.Name).ToList())
                            {
                                var count = db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                                               .SelectMany(p => db.Speeches
                                                   .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                                                    && s.SpeakerId == deputy.SpeakerId))
                                               .SelectMany(s => db.NamedEntity.Where(n => n.LemmaValue == entityChild.Name && n.NLPSpeechId == s.Id))
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

                        Log.Information($"Fraktion {graphFraction.Name} finished.");
                        if (graphFraction.Children.Count > 0)
                            root.Children.Add(graphFraction);
                    }
                    return root;
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while trying to recalculate the topic map: ");
                return null;
            }
        }
    }
}
