using BundestagMine.Models.Database;
using BundestagMine.Models.Database.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.Synchronisation.Services;
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
                        return 0;
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
                        if (db.Protocols.Any(p => p.LegislaturePeriod == protocol.LegislaturePeriod && p.Number == protocol.Number))
                        {
                            Log.Information($"Protocol {protocol.Title} is in the database already. Skipping it.");
                            counter++;
                            continue;
                        }
                        db.Protocols.Add(protocol);
                        if (ConfigManager.GetDeleteImportedEntity()) db.ImportedEntities.Remove(importedProtocol);

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
                            var speech = ParseImportedEntityToNLPSpeech(importedSpeech);

                            // This shouldn't happen, since we only import as whole protocols, but check if we imported this exact speech once before.
                            var alreadyStoredSpeech = db.NLPSpeeches.FirstOrDefault(s => s.ProtocolNumber == speech.ProtocolNumber
                                && s.AgendaItemNumber == speech.AgendaItemNumber && s.Text == speech.Text && s.SpeakerId == speech.SpeakerId);
                            if (alreadyStoredSpeech != null)
                            {
                                Log.Warning("Found a speech to a protocol which is already in the database - this should not happen!\n" +
                                    $"Speech Id: {alreadyStoredSpeech.Id}");
                                counter2++;
                                continue;
                            }

                            db.NLPSpeeches.Add(speech);
                            if (ConfigManager.GetDeleteImportedEntity()) db.ImportedEntities.Remove(importedSpeech);
                            counter2++;
                        }

                        Log.Information($"Saving the protocol and its speeches...");
                        await db.SaveChangesAsync();
                        Log.Information($"Saved!");
                        counter++;
                    }

                    // After the protocols, import potential new deputies
                    counter = 1;
                    var importedDeputies = db.ImportedEntities.Where(e => e.Type == ModelType.Deputy).ToList();
                    foreach (var importedDeputy in importedDeputies)
                    {
                        Log.Information($"Parsing deputy {counter}/{importedDeputies.Count}");
                        var deputy = ParseImportedEntityToDeputy(importedDeputy);

                        if (db.Deputies.Any(d => d.SpeakerId == deputy.SpeakerId))
                        {
                            Log.Information($"Deputy {deputy.FirstName + " " + deputy.LastName} is in the database already. Skipping it.");
                            counter++;
                            continue;
                        }

                        db.Deputies.Add(deputy);
                        if (ConfigManager.GetDeleteImportedEntity()) db.ImportedEntities.Remove(importedDeputy);

                        counter++;
                    }

                    await db.SaveChangesAsync();
                }

                return 1;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "There was an unknown error while importing new entities: ");
                return 0;
            }
        }

        /// <summary>
        /// Takes in a imported entity of type Deputy and creates a Deputy for our db out of it.
        /// </summary>
        /// <param name="importedEntity"></param>
        /// <returns></returns>
        private Deputy ParseImportedEntityToDeputy(ImportedEntity importedEntity)
        {
            if (importedEntity.Type != ModelType.Deputy) return null;

            var mongoDeputy = BsonDocument.Parse(importedEntity.ModelJson);
            var deputy = new Deputy();
            deputy.MongoId = Guid.Empty.ToString();

            if (mongoDeputy.AsBsonDocument.TryGetValue("id", out var id)) deputy.SpeakerId = id.AsString;
            if (mongoDeputy.AsBsonDocument.TryGetValue("firstName", out var firstName)) deputy.FirstName = firstName.AsString;
            if (mongoDeputy.AsBsonDocument.TryGetValue("lastName", out var lastName)) deputy.LastName = lastName.AsString;
            if (mongoDeputy.AsBsonDocument.TryGetValue("fraction", out var fraction)) deputy.Fraction = fraction.AsString;
            if (mongoDeputy.AsBsonDocument.TryGetValue("party", out var party)) deputy.Party = party.AsString;
            if (mongoDeputy.AsBsonDocument.TryGetValue("academicTitle", out var academicTitle)) deputy.AcademicTitle = academicTitle.AsString;
            if (mongoDeputy.AsBsonDocument.TryGetValue("historySince", out var historySince) && DateTime.TryParse(historySince.AsString, out var parsedHistorySince))
                deputy.HistorySince = parsedHistorySince;
            if (mongoDeputy.AsBsonDocument.TryGetValue("birthDate", out var birthDate) && DateTime.TryParse(birthDate.AsString, out var parsedBirthDate))
                deputy.BirthDate = parsedBirthDate;
            if (mongoDeputy.AsBsonDocument.TryGetValue("deathDate", out var deathDate) && DateTime.TryParse(deathDate.AsString, out var parsedDeathDate))
                deputy.DeathDate = parsedDeathDate;
            if (mongoDeputy.AsBsonDocument.TryGetValue("gender", out var gender)) deputy.Gender = gender.AsString;
            if (mongoDeputy.AsBsonDocument.TryGetValue("maritalStatus", out var maritalStatus)) deputy.MaritalStatus = maritalStatus.AsString;
            if (mongoDeputy.AsBsonDocument.TryGetValue("religion", out var religion)) deputy.Religion = religion.AsString;
            if (mongoDeputy.AsBsonDocument.TryGetValue("profession", out var profession)) deputy.Profession = profession.AsString;

            return deputy;
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
            speech.NamedEntities = new List<NamedEntity>();
            speech.Tokens = new List<Token>();
            speech.Sentiments = new List<Sentiment>();

            // Let the parsing begin
            var mongoResult = mongoSpeech.GetValue("result").AsBsonDocument;

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
                                Id = Guid.NewGuid(), // Have to give the shout a id by hand because of the annotation results
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

                        // When we have shouts, there should also be a result array in which the annotations of that shouts are.
                        if (mongoSegment.AsBsonDocument.TryGetValue("result", out var shoutResults) && shoutResults.AsBsonArray.Count > 0)
                        {
                            for (var y = 0; y < shoutResults.AsBsonArray.Count; y++)
                            {
                                // We get the current result and the shout this result belongs to.
                                var curResult = shoutResults.AsBsonArray[y].AsBsonDocument;
                                var curShout = shouts.ElementAt(y);

                                // Get each annotation from the result. This all is beyond cancerous, fuck MongoDB
                                if (curResult.TryGetValue("namedEntities", out var namedEntities))
                                {
                                    for (var x = 0; x < namedEntities.AsBsonArray.Count; x++)
                                    {
                                        var mongoEntity = namedEntities.AsBsonArray[x].AsBsonDocument;
                                        var entity = MongoDocumentsParserService.MongoNamedEntityToNamedEntity(mongoEntity);

                                        // Store the shout id to the named entity
                                        entity.ShoutId = curShout.Id;
                                        speech.NamedEntities.Add(entity);
                                    }
                                }
                                // Token
                                if (curResult.TryGetValue("tokens", out var shoutTokens))
                                {
                                    for (var x = 0; x < shoutTokens.AsBsonArray.Count; x++)
                                    {
                                        var mongoToken = shoutTokens.AsBsonArray[x].AsBsonDocument;
                                        var token = MongoDocumentsParserService.MongoTokenToToken(mongoToken);

                                        // Store the shout id to the named entity
                                        token.ShoutId = curShout.Id;
                                        speech.Tokens.Add(token);
                                    }
                                }
                                // Sentiments
                                if (curResult.TryGetValue("sentiments", out var shoutSentiments))
                                {
                                    // Sentiments are different for shouts and a bit bugged. There should only be once sentiment
                                    // for the whole shout - so its not based on sentences. Just take the first object of the array
                                    // in the array (Dumb) and that should be it...
                                    var shoutSentiment = shoutSentiments[0].AsBsonArray[0].AsBsonDocument;
                                    var sentiment = MongoDocumentsParserService.MongoSentimentToSentiment(shoutSentiment);
                                    sentiment.ShoutId = curShout.Id;
                                    speech.Sentiments.Add(sentiment);
                                }
                            }
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

            #region Tokens
            foreach (var mongoToken in mongoResult.GetValue("tokens").AsBsonArray)
            {
                speech.Tokens.Add(MongoDocumentsParserService.MongoTokenToToken(mongoToken.AsBsonDocument));
            }
            #endregion

            #region NE
            foreach (var mongoEntity in mongoResult.GetValue("namedEntities").AsBsonArray)
            {
                var entity = MongoDocumentsParserService.MongoNamedEntityToNamedEntity(mongoEntity.AsBsonDocument);
                speech.NamedEntities.Add(entity);
            }
            #endregion

            #region Sentiments
            foreach (var mongoSentimentArray in mongoResult.GetValue("sentiments").AsBsonArray)
            {
                var mongoSentiment = mongoSentimentArray.AsBsonArray[0];
                speech.Sentiments.Add(MongoDocumentsParserService.MongoSentimentToSentiment(mongoSentiment.AsBsonDocument));
            }
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
