using BundestagMine.Models.Database.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using BundestagMine.SqlDatabase;
using System.IO;

namespace BundestagMine.Synchronisation
{
    public class MongoDBExporter
    {
        private readonly IMongoDatabase _mongoDb;

        public MongoDBExporter(IMongoDatabase mongoDb)
        {
            _mongoDb = mongoDb;
        }

        /// <summary>
        /// The NE currenlty dont have their value such as Herr Spahn in their table. We need to readd them
        /// </summary>
        /// <returns></returns>
        public async Task FillNamedEntitiesFromTokens()
        {
            using (var sqlDb = new BundestagMineDbContext(ConfigManager.GetDbOptions()))
            {
                var counter = 0;
                var speeches = sqlDb.NLPSpeeches.Where(s => sqlDb.NamedEntity
                    .Any(ne => ne.NLPSpeechId == s.Id && ne.LemmaValue == null)).ToList();
                speeches.Reverse();
                foreach (var speech in speeches)
                {
                    var tokens = sqlDb.Token.Where(t => t.NLPSpeechId == speech.Id).ToList();
                    if (tokens.Count() == 0) continue;
                    
                    // ToList cause we change the collection, so we need to load it into the RAM
                    foreach(var namedEntity in sqlDb.NamedEntity.Where(ne => ne.NLPSpeechId == speech.Id).ToList())
                    {
                        if (namedEntity.LemmaValue != null) continue;
                        namedEntity.LemmaValue = string.Join(' ', tokens
                            .Where(t => t.Begin >= namedEntity.Begin && t.End <= namedEntity.End)
                            .Select(t => t.LemmaValue));
                    }
                    Console.WriteLine($"Saving NE for {tokens.Count()} tokens");
                    if(counter / 10 == 1)
                    {
                        await sqlDb.SaveChangesAsync();
                        Console.WriteLine("Saved");
                        counter = 0;
                    }
                    counter++;
                }
                await sqlDb.SaveChangesAsync();
            }             
        }

        /// <summary>
        /// Fetches all speeches in the MongoDB and puts them correctly into the mssql database
        /// </summary>
        /// <returns></returns>
        public async Task ExportSpeeches()
        {
            try
            {
                using (var sqlDb = new BundestagMineDbContext(ConfigManager.GetDbOptions()))
                {
                    var mongoSpeeches = _mongoDb.GetCollection<BsonDocument>("speeches").AsQueryable();
                    var counter = 0;
                    foreach (var mongoSpeech in mongoSpeeches)
                    {
                        var speech = new Speech();
                        speech.MongoId = mongoSpeech.GetValue("_id").ToString();
                        if (sqlDb.Speeches.Any(s => s.MongoId == speech.MongoId))
                        {
                            Console.WriteLine("Skipping speech");
                            continue;
                        }
                        speech.Text = mongoSpeech.GetValue("text").AsString;
                        if (mongoSpeech.TryGetValue("speakerId", out var speakerId)) speech.SpeakerId = speakerId.AsString;

                        // AgendaItem
                        var mongoAngedaItem = mongoSpeech.GetValue("agendaItem").AsBsonDocument;
                        speech.LegislaturePeriod = mongoAngedaItem.GetValue("legislaturePeriod").AsInt32;
                        speech.ProtocolNumber = mongoAngedaItem.GetValue("protocol").AsInt32;
                        speech.AgendaItemNumber = mongoAngedaItem.GetValue("number").AsInt32;

                        // Segments
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

                        sqlDb.Speeches.Add(speech);
                        Console.WriteLine(counter);
                        // Lets save every 100 speeches.
                        if (counter / 1000 == 1)
                        {
                            counter = 0;
                            await sqlDb.SaveChangesAsync();
                            Console.WriteLine($"Saving db / {sqlDb.Speeches.Count()} Speeches.");
                        }
                        counter++;
                    }
                    await sqlDb.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Fetches all deputies in the MongoDB and puts them correctly into the mssql database
        /// </summary>
        /// <returns></returns>
        public async Task ExportDeputies()
        {
            try
            {
                using (var sqlDb = new BundestagMineDbContext(ConfigManager.GetDbOptions()))
                {
                    var mongoDeputies = _mongoDb.GetCollection<BsonDocument>("deputies").AsQueryable();
                    foreach (var mongoDeputy in mongoDeputies)
                    {
                        var deputy = new Deputy();
                        deputy.MongoId = mongoDeputy.GetValue("_id").ToString();
                        if (sqlDb.Deputies.Any(s => s.MongoId == deputy.MongoId))
                        {
                            Console.WriteLine("Skipping speech");
                            continue;
                        }
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

                        sqlDb.Deputies.Add(deputy);
                    }
                    Console.WriteLine("Saving");
                    await sqlDb.SaveChangesAsync();
                    Console.WriteLine("Finished");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Fetches all protocls in the MongoDB and puts them correctly into the mssql database
        /// </summary>
        /// <returns></returns>
        public async Task ExportProtocols()
        {
            using (var sqlDb = new BundestagMineDbContext(ConfigManager.GetDbOptions()))
            {
                var mongoProtocols = _mongoDb.GetCollection<BsonDocument>("protocols").AsQueryable();
                foreach (var mongoProtocol in mongoProtocols)
                {
                    var protocol = new Protocol();
                    protocol.MongoId = mongoProtocol.GetValue("_id").ToString();

                    if (sqlDb.Protocols.Any(s => s.MongoId == protocol.MongoId))
                    {
                        Console.WriteLine("Skipping speech");
                        continue;
                    }

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

                    sqlDb.Protocols.Add(protocol);
                    await sqlDb.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Fetches all networkdata in the MongoDB and puts them correctly into the mssql database
        /// </summary>
        /// <returns></returns>
        public async Task ExportNetworkData()
        {
            using (var sqlDb = new BundestagMineDbContext(ConfigManager.GetDbOptions()))
            {
                // We cant deserilize the obj to networkdata directly because the Ids of the nodes are bugged then...
                var mongoNetworkDatas = _mongoDb.GetCollection<BsonDocument>("networkdata").AsQueryable();
                foreach (var mongoNetworkData in mongoNetworkDatas)
                {
                    var networkData = new NetworkData();
                    networkData.MongoId = mongoNetworkData.GetValue("_id").ToString();

                    if (sqlDb.NetworkDatas.Any(s => s.MongoId == networkData.MongoId))
                    {
                        Console.WriteLine("Skipping speech");
                        continue;
                    }

                    // Nodes
                    var nodes = new List<CommentNetworkNode>();
                    var mongoNodes = mongoNetworkData.GetValue("nodes").AsBsonArray;
                    foreach (var mongoNode in mongoNodes)
                    {
                        var node = new CommentNetworkNode();
                        if (mongoNode.AsBsonDocument.TryGetValue("id", out var id)) node.Id = id.AsString;
                        if (mongoNode.AsBsonDocument.TryGetValue("name", out var name)) node.Name = name.AsString;
                        if (mongoNode.AsBsonDocument.TryGetValue("party", out var party)) node.Party = party.AsString;

                        nodes.Add(node);
                    }

                    // Links
                    var links = new List<CommentNetworkLink>();
                    var mongoLinks = mongoNetworkData.GetValue("links").AsBsonArray;
                    foreach (var mongoLink in mongoLinks)
                    {
                        var link = new CommentNetworkLink();
                        if (mongoLink.AsBsonDocument.TryGetValue("source", out var source)) link.Source = source.AsString;
                        if (mongoLink.AsBsonDocument.TryGetValue("target", out var target)) link.Target = target.AsString;
                        if (mongoLink.AsBsonDocument.TryGetValue("sentiment", out var sentiment)) link.Sentiment = sentiment.AsDouble;
                        if (mongoLink.AsBsonDocument.TryGetValue("value", out var value)) link.Value = value.AsInt32;

                        links.Add(link);
                    }

                    // Add them
                    networkData.Nodes = nodes;
                    networkData.Links = links;

                    sqlDb.Add(networkData);
                    await sqlDb.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Fetches all nlpspeeches from local disc and puts them correctly into the mssql database
        /// </summary>
        /// <returns></returns>
        public async Task ExportNLPSpeeches()
        {
            var counter = 0;
            using (var sqlDb = new BundestagMineDbContext(ConfigManager.GetDbOptions()))
            {
                foreach (string file in Directory.EnumerateFiles(ConfigManager.GetLocalDataDirectory() + "SpeechesNLP", "*.json"))
                {
                    var curJsonContent = File.ReadAllText(file);
                    var mongoSpeech = BsonDocument.Parse(curJsonContent);

                    var speech = new NLPSpeech();
                    speech.MongoId = mongoSpeech.GetValue("_id").ToString();
                    var alreadyExistingSpeech = sqlDb.NLPSpeeches.FirstOrDefault(s => s.MongoId == speech.MongoId);
                    // NLP results
                    var mongoResult = mongoSpeech.GetValue("result").AsBsonDocument;
                    // Fixing errors of the past :-)
                    if (alreadyExistingSpeech != null)
                    {
                        Console.WriteLine("Skipping speech");

                        if (!sqlDb.Token.Any(t => t.NLPSpeechId == alreadyExistingSpeech.Id))
                        {
                            Console.WriteLine($"Readding tokens for {alreadyExistingSpeech.Id}");
                            // Dont do this at home kids.
                            var tokens2 = new List<Token>();
                            foreach (var mongoToken in mongoResult.GetValue("tokens").AsBsonArray)
                            {
                                var token = new Token();
                                token.NLPSpeechId = alreadyExistingSpeech.Id;
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
                                tokens2.Add(token);
                            }
                            await sqlDb.Token.AddRangeAsync(tokens2);
                            await sqlDb.SaveChangesAsync();
                        }
                        continue;
                    }
                    speech.Text = mongoSpeech.GetValue("text").AsString;
                    if (mongoSpeech.TryGetValue("speakerId", out var speakerId)) speech.SpeakerId = speakerId.AsString;

                    // AgendaItem
                    var mongoAngedaItem = mongoSpeech.GetValue("agendaItem").AsBsonDocument;
                    speech.LegislaturePeriod = mongoAngedaItem.GetValue("legislaturePeriod").AsInt32;
                    speech.ProtocolNumber = mongoAngedaItem.GetValue("protocol").AsInt32;
                    speech.AgendaItemNumber = mongoAngedaItem.GetValue("number").AsInt32;

                    // Segments
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

                    // Categories
                    var categories = new List<CategoryCoveredTagged>();
                    foreach (var mongoCategory in mongoResult.GetValue("categoryCoveredTagged").AsBsonArray)
                    {
                        var category = new CategoryCoveredTagged();
                        if (mongoCategory.AsBsonDocument.TryGetValue("begin", out var begin)) category.Begin = begin.AsInt32;
                        if (mongoCategory.AsBsonDocument.TryGetValue("end", out var end)) category.End = end.AsInt32;
                        if (mongoCategory.AsBsonDocument.TryGetValue("value", out var value)) category.Value = value.AsString;
                        try
                        {
                            if (mongoCategory.AsBsonDocument.TryGetValue("score", out var score)) category.Score = score.AsDouble;
                        }
                        catch (Exception ex) { Console.WriteLine("Parsing wieder " + ex); }
                        categories.Add(category);
                    }
                    speech.CategoryCoveredTags = categories;

                    // entities
                    var entities = new List<NamedEntity>();
                    foreach (var mongoEntity in mongoResult.GetValue("namedEntities").AsBsonArray)
                    {
                        var entity = new NamedEntity();
                        if (mongoEntity.AsBsonDocument.TryGetValue("begin", out var begin)) entity.Begin = begin.AsInt32;
                        if (mongoEntity.AsBsonDocument.TryGetValue("end", out var end)) entity.End = end.AsInt32;
                        if (mongoEntity.AsBsonDocument.TryGetValue("value", out var value)) entity.Value = value.AsString;
                        entities.Add(entity);
                    }
                    speech.NamedEntities = entities;

                    // tokens
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

                    // Sentiments
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

                    sqlDb.NLPSpeeches.Add(speech);
                    Console.WriteLine(counter);
                    if (counter / 10 == 1)
                    {
                        await sqlDb.SaveChangesAsync();
                        Console.WriteLine("Saved");
                        counter = 0;
                    }

                    counter++;
                }
                await sqlDb.SaveChangesAsync();
            }
        }
    }
}
