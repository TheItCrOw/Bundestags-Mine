using BundestagMine.Models.Database;
using BundestagMine.Models.Database.MongoDB;
using BundestagMine.SqlDatabase;
using MongoDB.Bson;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Synchronisation
{
    /// <summary>
    /// Checks if there are new entities in the Imported entities table and parses them into correct relational models
    /// </summary>
    public class ImportedEntityParser
    {
        /// <summary>
        /// Parses new entities if there are any in the ImportedEntities table
        /// </summary>
        public async Task<int> ParseNewPotentialEntities()
        {
            try
            {
                using (var db = new BundestagMineDbContext(ConfigManager.GetDbOptions()))
                {
                    // Check if there are any new entities
                    if (!db.ImportedEntities.Any())
                    {
                        Log.Information("No new entities found to import. Aborting.");
                        return 1;
                    }

                    var totalNewProtocols = db.ImportedEntities.Where(e => e.Type == ModelType.Protocol).Count();
                    Log.Information($"Found {totalNewProtocols} new protocols.");
                    Log.Information($"Found {db.ImportedEntities.Where(e => e.Type == ModelType.NLPSpeech).Count()} new speeches.");
                    Log.Information($"Found {db.ImportedEntities.Where(e => e.Type == ModelType.Deputy).Count()} new Deputies.");

                    // Else import each new protocol at a time.
                    // We order by descending, because the JAVA service puts the NEWEST protocols in first, but we want to
                    // parse the old ones first for right order.
                    var counter = 1;
                    foreach (var importedProtocol in db.ImportedEntities.Where(e => e.Type == ModelType.Protocol)
                        .OrderByDescending(p => p.ImportedDate)
                        .ToList())
                    {
                        Log.Information($"Parsing protocol {counter}/{totalNewProtocols}");

                        // we need to check whether this protocol is already in the database by some accident.
                        var protocol = ParseImportedEntityToProtocol(importedProtocol);
                        if(db.Protocols.Any(p => p.LegislaturePeriod == protocol.LegislaturePeriod && p.Number == protocol.Number))
                        {
                            Log.Information($"Protocol {protocol.Title} is in the database already. Skipping it.");
                            counter++;
                            continue;
                        }
                        db.Protocols.Add(protocol);

                        // The Speeches of this protocol
                        var importedSpeeches = db.ImportedEntities
                            .Where(e => e.Type == ModelType.NLPSpeech && e.ProtocolId == importedProtocol.Id)
                            .ToList();

                        // Now parse each nlp speech
                        var speeches = new List<NLPSpeech>();
                        var counter2 = 1;
                        foreach (var importedSpeech in importedSpeeches)
                        {
                            Log.Information($"Parsing speech {counter2}/{importedSpeeches.Count}");
                            db.NLPSpeeches.Add(ParseImportedEntityToNLPSpeech(importedSpeech));
                            counter2++;
                        }

                        Log.Information($"Saving the protocol and its speeches...");
                        await db.SaveChangesAsync();
                        Log.Information($"Saved!");
                        counter++;
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                Log.Error("There was an error while importing new entities: ", ex);
                return 0;
            }
        }

        /// <summary>
        /// Takes in a imported entity of type NLPSpeech and creates a NLPSpeech for our db out of it.
        /// </summary>
        /// <param name="importedEntity"></param>
        /// <returns></returns>
        private NLPSpeech ParseImportedEntityToNLPSpeech(ImportedEntity importedEntity)
        {
            if (importedEntity.Type != ModelType.NLPSpeech) return null;

            var mongoSpeech = BsonDocument.Parse(importedEntity.ModelJson);
            var speech = new NLPSpeech();
            speech.MongoId = Guid.Empty.ToString();

            // Let the parsing begin
            var mongoResult = mongoSpeech.GetValue("result").AsBsonDocument;

            #region Tokens
            var tokens = new List<Token>();
            foreach (var mongoToken in mongoResult.GetValue("tokens").AsBsonArray)
            {
                var token = new Token();
                if (mongoToken.AsBsonDocument.TryGetValue("begin", out var begin)) token.Begin = begin.AsInt32;
                if (mongoToken.AsBsonDocument.TryGetValue("end", out var end)) token.End = end.AsInt32;
                if (mongoToken.AsBsonDocument.TryGetValue("lemmaValue", out var lemmaValue)) token.LemmaValue = lemmaValue.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("morph", out var morph) && bool.TryParse(morph.AsString, out var bo))
                {
                    token.Morph = bo;
                }
                if (mongoToken.AsBsonDocument.TryGetValue("gender", out var gender)) token.Gender = gender.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("stem", out var stem)) token.Stem = stem.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("posValue", out var posValue)) token.posValue = posValue.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("number", out var number)) token.Number = number.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("case", out var case2)) token.Case = case2.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("degree", out var degree)) token.Degree = degree.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("verbForm", out var verbForm)) token.VerbForm = verbForm.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("tense", out var tense)) token.Tense = tense.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("mood", out var mood)) token.Mood = mood.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("voice", out var voice)) token.Voice = voice.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("definiteness", out var definiteness)) token.Definiteness = definiteness.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("value", out var value)) token.Value = value.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("person", out var person)) token.Person = person.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("aspect", out var aspect)) token.Aspect = aspect.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("animacy", out var animacy)) token.Animacy = animacy.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("negative", out var negative)) token.Negative = negative.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("possessive", out var possessive)) token.Possessive = possessive.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("pronType", out var pronType)) token.PronType = pronType.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("reflex", out var reflex)) token.Reflex = reflex.AsString;
                if (mongoToken.AsBsonDocument.TryGetValue("transitivity", out var transitivity)) token.Transitivity = transitivity.AsString;
                tokens.Add(token);
            }
            speech.Tokens = tokens;
            #endregion

            #region Text
            // Text and speaker id
            speech.Text = mongoSpeech.GetValue("text").AsString;
            if (mongoSpeech.TryGetValue("speakerId", out var speakerId)) speech.SpeakerId = speakerId.AsString;
            #endregion

            #region AgendaItem
            var mongoAngedaItem = mongoSpeech.GetValue("agendaItem").AsBsonDocument;
            speech.LegislaturePeriod = mongoAngedaItem.GetValue("legislaturePeriod").AsInt32;
            speech.ProtocolNumber = mongoAngedaItem.GetValue("protocol").AsInt32;
            speech.AgendaItemNumber = mongoAngedaItem.GetValue("number").AsInt32;
            #endregion

            #region Segments
            if (mongoSpeech.TryGetValue("segments", out var mongoSegments) && mongoSegments.AsBsonDocument.ElementCount > 0)
            {
                var segments = new List<SpeechSegment>();
                for (int i = 0; i < mongoSegments.AsBsonDocument.ElementCount; i++)
                {
                    var mongoSegment = mongoSegments.AsBsonDocument[i].AsBsonArray[0].AsBsonDocument;
                    var segment = new SpeechSegment()
                    {
                        Text = mongoSegment.GetValue("text").AsString
                    };
                    // potential shouts
                    if (mongoSegment.AsBsonDocument.TryGetValue("shouts", out var mongoShouts) && mongoShouts.AsBsonDocument.ElementCount > 0)
                    {
                        var shouts = new List<Shout>();
                        for (int k = 0; k < mongoShouts.AsBsonDocument.ElementCount; k++)
                        {
                            var mongoShout = mongoShouts.AsBsonDocument[k].AsBsonArray[0].AsBsonDocument;
                            var shout = new Shout()
                            {
                                Text = mongoShout.AsBsonDocument.GetValue("text").AsString
                            };
                            if (mongoShout.AsBsonDocument.TryGetValue("by", out var by))
                            {
                                if (by.AsBsonDocument.TryGetValue("firstName", out var firstName)) shout.FirstName = firstName.AsString;
                                if (by.AsBsonDocument.TryGetValue("lastName", out var lastName)) shout.LastName = lastName.AsString;
                                if (by.AsBsonDocument.TryGetValue("fraction", out var fraction)) shout.Fraction = fraction.AsString;
                                if (by.AsBsonDocument.TryGetValue("party", out var party)) shout.Party = party.AsString;
                                if (by.AsBsonDocument.TryGetValue("id", out var speakerId2)) shout.SpeakerId = speakerId2.AsString;
                            }

                            shouts.Add(shout);
                        }
                        segment.Shouts = shouts;
                    }
                    segments.Add(segment);
                }
                speech.Segments = segments;
            }
            #endregion

            #region categories
            //var categories = new List<CategoryCoveredTagged>();
            //foreach (var mongoCategory in mongoResult.GetValue("categoryCoveredTagged").AsBsonArray)
            //{
            //    var category = new CategoryCoveredTagged();
            //    if (mongoCategory.AsBsonDocument.TryGetValue("begin", out var begin)) category.Begin = begin.AsInt32;
            //    if (mongoCategory.AsBsonDocument.TryGetValue("end", out var end)) category.End = end.AsInt32;
            //    if (mongoCategory.AsBsonDocument.TryGetValue("value", out var value)) category.Value = value.AsString;
            //    try
            //    {
            //        if (mongoCategory.AsBsonDocument.TryGetValue("score", out var score)) category.Score = score.AsDouble;
            //    }
            //    catch (Exception ex) { Console.WriteLine("Parsing wieder " + ex); }
            //    categories.Add(category);
            //}
            //speech.CategoryCoveredTags = categories;
            #endregion

            #region NE
            var entities = new List<NamedEntity>();
            foreach (var mongoEntity in mongoResult.GetValue("namedEntities").AsBsonArray)
            {
                var entity = new NamedEntity();
                if (mongoEntity.AsBsonDocument.TryGetValue("begin", out var begin)) entity.Begin = begin.AsInt32;
                if (mongoEntity.AsBsonDocument.TryGetValue("end", out var end)) entity.End = end.AsInt32;
                if (mongoEntity.AsBsonDocument.TryGetValue("value", out var value)) entity.Value = value.AsString;
                if (mongoEntity.AsBsonDocument.TryGetValue("coveredText", out var coveredText)) entity.LemmaValue = coveredText.AsString;
                entities.Add(entity);
            }
            speech.NamedEntities = entities;
            #endregion

            #region Sentiments
            var sentiments = new List<Sentiment>();
            foreach (var mongoSentimentArray in mongoResult.GetValue("sentiments").AsBsonArray)
            {
                var mongoSentiment = mongoSentimentArray.AsBsonArray[0];
                var sentiment = new Sentiment();
                if (mongoSentiment.AsBsonDocument.TryGetValue("begin", out var begin)) sentiment.Begin = begin.AsInt32;
                if (mongoSentiment.AsBsonDocument.TryGetValue("end", out var end)) sentiment.End = end.AsInt32;
                if (mongoSentiment.AsBsonDocument.TryGetValue("sentimentSingleScore", out var sentimentSingleScore))
                {
                    var test = sentimentSingleScore.AsString.Replace(".", ",");
                    sentiment.SentimentSingleScore = double.Parse(test);
                }
                sentiments.Add(sentiment);
            }
            speech.Sentiments = sentiments;
            #endregion

            return speech;
        }

        /// <summary>
        /// Takes in a imported entity of type Protocol and creates a Protocol for our db out of it.
        /// </summary>
        /// <param name="importedEntity"></param>
        /// <returns></returns>
        private Protocol ParseImportedEntityToProtocol(ImportedEntity importedEntity)
        {
            if (importedEntity.Type != ModelType.Protocol) return null;

            // We parse the json to a bson document - its easier to handle it that way
            var mongoProtocol = BsonDocument.Parse(importedEntity.ModelJson);
            var protocol = new Protocol();
            protocol.MongoId = Guid.Empty.ToString();

            // Now parse that boi
            // Whatever the fuck this dateformat is, we have to change it...
            var date = mongoProtocol.GetValue("date").AsString;
            var splited = date.Split(' ');
            date = $"{splited[0]} {splited[1]} {splited[2]} {splited[5]} {splited[3]} GMT+0000 (GMT Standard Time)";
            protocol.Date = DateTime.ParseExact(date,
                "ddd MMM dd yyyy HH:mm:ss 'GMT+0000 (GMT Standard Time)'",
                CultureInfo.InvariantCulture);

            protocol.AgendaItemsCount = mongoProtocol.GetValue("agendaItemsCount").AsInt32;
            protocol.LegislaturePeriod = mongoProtocol.GetValue("legislaturePeriod").AsInt32;
            protocol.Number = mongoProtocol.GetValue("number").AsInt32;
            protocol.Title = mongoProtocol.GetValue("title").AsString;

            return protocol;
        }
    }
}
