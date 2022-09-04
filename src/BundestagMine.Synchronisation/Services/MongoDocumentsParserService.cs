using BundestagMine.Models.Database.MongoDB;
using BundestagMine.Utility;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.Synchronisation.Services
{
    /// <summary>
    /// Service that parses given mongo documents to their regular sql db objects
    /// </summary>
    public class MongoDocumentsParserService
    {
        public static Sentiment MongoSentimentToSentiment(BsonDocument mongoSentiment)
        {
            var sentiment = new Sentiment();
            if (mongoSentiment.AsBsonDocument.TryGetValue("begin", out var begin)) sentiment.Begin = begin.AsInt32;
            if (mongoSentiment.AsBsonDocument.TryGetValue("end", out var end)) sentiment.End = end.AsInt32;
            if (mongoSentiment.AsBsonDocument.TryGetValue("sentimentSingleScore", out var sentimentSingleScore))
            {
                var test = sentimentSingleScore.AsString.Replace(".", ",");
                sentiment.SentimentSingleScore = double.Parse(test);
            }
            return sentiment;
        }

        public static NamedEntity MongoNamedEntityToNamedEntity(BsonDocument mongoEntity)
        {
            var entity = new NamedEntity();
            if (mongoEntity.AsBsonDocument.TryGetValue("begin", out var begin)) entity.Begin = begin.AsInt32;
            if (mongoEntity.AsBsonDocument.TryGetValue("end", out var end)) entity.End = end.AsInt32;
            if (mongoEntity.AsBsonDocument.TryGetValue("value", out var value)) entity.Value = value.AsString;
            if (mongoEntity.AsBsonDocument.TryGetValue("coveredText", out var coveredText)) entity.LemmaValue = coveredText.AsString.ToCleanNE();

            return entity;
        }

        public static Token MongoTokenToToken(BsonDocument mongoToken)
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

            return token;
        }
    }
}
