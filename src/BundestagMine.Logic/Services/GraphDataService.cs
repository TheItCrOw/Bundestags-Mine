using BundestagMine.Models.Database.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.Logic.Services
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

    public class GraphDataService
    {
        private readonly MetadataService _metadataService;
        private readonly BundestagMineDbContext _db;

        public GraphDataService(BundestagMineDbContext db, MetadataService metadataService)
        {
            _metadataService = metadataService;
            _db = db;
        }

        /// <summary>
        /// Builds and returns the actual comment network data of a protocol
        /// </summary>
        /// <param name="period"></param>
        /// <param name="meetingNumber"></param>
        /// <returns></returns>
        public NetworkData GetActualCommentNetworkOfProtocol(int period, int meetingNumber)
        {
            var networkData = new NetworkData();
            networkData.Id = Guid.NewGuid();
            networkData.Nodes = new List<CommentNetworkNode>();
            networkData.Links = new List<CommentNetworkLink>();

            var speechesOfProtocol = _metadataService.GetSpeechesOfProtocol(period, meetingNumber).ToList();
            // Let's go through all speeches, check their shouts and then create the networkdata
            // accordingly
            foreach(var speech in speechesOfProtocol)
            {
                var actualComments = _metadataService.GetActualCommentsOfSpeech(speech);
                if (string.IsNullOrEmpty(speech.SpeakerId)) continue;
                var speaker = _db.Deputies.FirstOrDefault(d => d.SpeakerId == speech.SpeakerId);

                foreach(var shout in actualComments)
                {
                    if (string.IsNullOrEmpty(shout.SpeakerId)) continue;

                    var shouter = _db.Deputies.FirstOrDefault(d => d.SpeakerId == shout.SpeakerId);

                    if (shouter == default || speaker == default) continue;
                    // When we found a shout, both speakers have to be stored in the node list
                    // First the speaker
                    if (!networkData.Nodes.Any(n => n.Id == speaker.SpeakerId))
                        networkData.Nodes.Add(new CommentNetworkNode()
                        {
                            Id = speaker.SpeakerId,
                            Name = speaker.GetFullName(),
                            Party = speaker.GetOrga(),
                            NetworkDataId = networkData.Id
                        });

                    // Then the shouter
                    if (!networkData.Nodes.Any(n => n.Id == shouter.SpeakerId))
                        networkData.Nodes.Add(new CommentNetworkNode()
                        {
                            Id = shouter.SpeakerId,
                            Name = shouter.GetFullName(),
                            Party = shouter.GetOrga(),
                            NetworkDataId = networkData.Id
                        });

                    // Whats the sentiment of the shout?
                    var shoutSentiment = _db.Sentiment
                        .Where(s => s.NLPSpeechId == speech.Id && s.ShoutId == shout.Id)
                        .AverageOrDefault(s => s.SentimentSingleScore);

                    // When we have both nodes savely added, now update or create their links.
                    var existingLink = networkData.Links
                        .FirstOrDefault(l => l.Source == shouter.SpeakerId && l.Target == speaker.SpeakerId);
                    if (existingLink != default)
                    {
                        existingLink.Value++;
                        existingLink.Sentiment = (existingLink.Sentiment + shoutSentiment) / existingLink.Value;
                    }
                    else
                    {
                        networkData.Links.Add(new CommentNetworkLink()
                        {
                            NetworkDataId = networkData.Id,
                            Sentiment = shoutSentiment,
                            Source = shouter.SpeakerId,
                            Target = speaker.SpeakerId,
                            Value = 1
                        });
                    }
                }
            }

            return networkData;
        }

    }
}
