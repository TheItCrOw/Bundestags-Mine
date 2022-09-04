using BundestagMine.Models.Database.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.Utility;
using BundestagMine.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.Services
{
    public class AnnotationService
    {
        private readonly BundestagScraperService _bundestagScraperService;
        private readonly MetadataService _metadataService;
        private readonly BundestagMineDbContext _db;

        public AnnotationService(BundestagMineDbContext db,
            MetadataService metadataService,
            BundestagScraperService bundestagScraperService)
        {
            _bundestagScraperService = bundestagScraperService;
            _metadataService = metadataService;
            _db = db;
        }

        /// <summary>
        /// Builds reduced Element, Count models for the tokens and returns them.
        /// Pass in the parameters as required.
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="fraction"></param>
        /// <param name="party"></param>
        /// <param name="speakerId"></param>
        /// <returns></returns>
        public dynamic GetTokensForGraphs(
            int limit,
            DateTime from,
            DateTime to,
            string fraction,
            string party,
            string speakerId)
        {
            if (fraction != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && _db.Deputies.SingleOrDefault(d => d.SpeakerId == s.SpeakerId).Fraction == fraction))
                .SelectMany(s => _db.Token.Where(t => t.LemmaValue != null && s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty))
                .GroupBy(t => t.LemmaValue)
                .Select(t => new { Element = t.Key, Count = t.Count() })
                .OrderByDescending(kv => kv.Count)
                .Take(limit)
                .ToList();
            else if (party != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && _db.Deputies.SingleOrDefault(d => d.SpeakerId == s.SpeakerId).Party == party))
                .SelectMany(s => _db.Token.Where(t => t.LemmaValue != null && s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty))
                .GroupBy(t => t.LemmaValue)
                .Select(t => new { Element = t.Key, Count = t.Count() })
                .OrderByDescending(kv => kv.Count)
                .Take(limit)
                .ToList();
            else if (speakerId != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && s.SpeakerId == speakerId))
                .SelectMany(s => _db.Token.Where(t => t.LemmaValue != null && s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty))
                .GroupBy(t => t.LemmaValue)
                .Select(t => new { Element = t.Key, Count = t.Count() })
                .OrderByDescending(kv => kv.Count)
                .Take(limit)
                .ToList();
            else
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches.Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod))
                .SelectMany(s => _db.Token.Where(t => t.LemmaValue != null && s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty))
                .GroupBy(t => t.LemmaValue)
                .Select(t => new { Element = t.Key, Count = t.Count() })
                .OrderByDescending(kv => kv.Count)
                .Take(limit)
                .ToList();
        }

        /// <summary>
        /// Builds reduced Element, Count models for the pos and returns them.
        /// Pass in the parameters as required.
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="fraction"></param>
        /// <param name="party"></param>
        /// <param name="speakerId"></param>
        /// <returns></returns>
        public dynamic GetPOSForGraphs(
            int limit,
            DateTime from,
            DateTime to,
            string fraction,
            string party,
            string speakerId)
        {
            if (fraction != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && _db.Deputies.SingleOrDefault(d => d.SpeakerId == s.SpeakerId).Fraction == fraction))
                .SelectMany(s => _db.Token.Where(t => t.posValue != null && s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty))
                .GroupBy(t => t.posValue)
                .Select(t => new { Element = t.Key, Count = t.Count() })
                .OrderByDescending(kv => kv.Count)
                .Take(limit)
                .ToList();
            else if (party != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && _db.Deputies.SingleOrDefault(d => d.SpeakerId == s.SpeakerId).Party == party))
                .SelectMany(s => _db.Token.Where(t => t.posValue != null && s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty))
                .GroupBy(t => t.posValue)
                .Select(t => new { Element = t.Key, Count = t.Count() })
                .OrderByDescending(kv => kv.Count)
                .Take(limit)
                .ToList();
            else if (speakerId != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && s.SpeakerId == speakerId))
                .SelectMany(s => _db.Token.Where(t => t.posValue != null && s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty))
                .GroupBy(t => t.posValue)
                .Select(t => new { Element = t.Key, Count = t.Count() })
                .OrderByDescending(kv => kv.Count)
                .Take(limit)
                .ToList();
            else
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches.Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod))
                .SelectMany(s => _db.Token.Where(t => t.posValue != null && s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty))
                .GroupBy(t => t.posValue)
                .Select(t => new { Element = t.Key, Count = t.Count() })
                .OrderByDescending(kv => kv.Count)
                .Take(limit)
                .ToList();
        }

        /// <summary>
        /// Takes in a sentiment score and returns the corresponding neg, neu, pos string
        /// </summary>
        /// <returns></returns>
        private string SentimentScoreToString(double score)
        {
            if (score < 0) return "neg";
            else if (score == 0) return "neu";
            else if (score > 0) return "pos";

            return "neu";
        }

        /// <summary>
        /// Builds reduced models for the sentiments and returns them.
        /// Pass in the parameters as required.
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="fraction"></param>
        /// <param name="party"></param>
        /// <param name="speakerId"></param>
        /// <returns></returns>
        public dynamic GetSentimentsForGraphs(
            DateTime from,
            DateTime to,
            string fraction,
            string party,
            string speakerId)
        {
            if (fraction != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && _db.Deputies.SingleOrDefault(d => d.SpeakerId == s.SpeakerId).Fraction == fraction))
                .SelectMany(s => _db.Sentiment.Where(t => s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty))
                .AsEnumerable()
                .GroupBy(t => SentimentScoreToString(t.SentimentSingleScore))
                .Select(t => new { Element = t.Key, Count = t.Count() })
                .ToList();
            else if (party != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && _db.Deputies.SingleOrDefault(d => d.SpeakerId == s.SpeakerId).Party == party))
                .SelectMany(s => _db.Sentiment.Where(t => s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty))
                .AsEnumerable()
                .GroupBy(t => SentimentScoreToString(t.SentimentSingleScore))
                .Select(t => new { Element = t.Key, Count = t.Count() })
                .ToList();
            else if (speakerId != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && s.SpeakerId == speakerId))
                .SelectMany(s => _db.Sentiment.Where(t => s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty))
                .AsEnumerable()
                .GroupBy(t => SentimentScoreToString(t.SentimentSingleScore))
                .Select(t => new { Element = t.Key, Count = t.Count() })
                .ToList();
            else
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches.Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod))
                .SelectMany(s => _db.Sentiment.Where(t => s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty))
                .AsEnumerable()
                .GroupBy(t => SentimentScoreToString(t.SentimentSingleScore))
                .Select(t => new { Element = t.Key, Count = t.Count() })
                .ToList();
        }

        /// <summary>
        /// Builds reduced Element, Count models for the named entities and returns them.
        /// Pass in the parameters as required.
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="fraction"></param>
        /// <param name="party"></param>
        /// <param name="speakerId"></param>
        /// <returns></returns>
        public dynamic GetNamedEntitiesForGraph(
            int limit,
            DateTime from,
            DateTime to,
            string fraction,
            string party,
            string speakerId)
        {
            if (fraction != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && _db.Deputies.SingleOrDefault(d => d.SpeakerId == s.SpeakerId).Fraction == fraction))
                .SelectMany(s => _db.NamedEntity.Where(t => s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty 
                                && !TopicHelper.TopicBlackList.Contains(t.LemmaValue)))
                .AsEnumerable()
                .GroupBy(n => n.Value)
                .Select(g1 => new
                {
                    Type = g1.Key,
                    Count = g1.Count(),
                    Value = g1
                    .GroupBy(n => n.LemmaValue?.ToLower())
                    .Take(limit)
                    .Select(g2 => new { Value = g2.Key, Count = g2.Count() })
                    .OrderByDescending(n => n.Count)
                })
                .Take(limit)
                .ToList();
            else if (party != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && _db.Deputies.SingleOrDefault(d => d.SpeakerId == s.SpeakerId).Party == party))
                .SelectMany(s => _db.NamedEntity.Where(t => s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty 
                            && !TopicHelper.TopicBlackList.Contains(t.LemmaValue)))
                .AsEnumerable()
                .GroupBy(n => n.Value)
                .Select(g1 => new
                {
                    Type = g1.Key,
                    Count = g1.Count(),
                    Value = g1
                    .GroupBy(n => n.LemmaValue?.ToLower())
                    .Take(limit)
                    .Select(g2 => new { Value = g2.Key, Count = g2.Count() })
                    .OrderByDescending(n => n.Count)
                })
                .Take(limit)
                .ToList();
            else if (speakerId != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.SpeakerId == speakerId && s.LegislaturePeriod == p.LegislaturePeriod))
                .SelectMany(s => _db.NamedEntity.Where(t => s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty 
                            && !TopicHelper.TopicBlackList.Contains(t.LemmaValue)))
                .AsEnumerable()
                .GroupBy(n => n.Value)
                .Select(g1 => new
                {
                    Type = g1.Key,
                    Count = g1.Count(),
                    Value = g1
                    .GroupBy(n => n.LemmaValue?.ToLower())
                    .Take(limit)
                    .Select(g2 => new { Value = g2.Key, Count = g2.Count() })
                    .OrderByDescending(n => n.Count)
                })
                .Take(limit)
                .ToList();
            else
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod))
                .SelectMany(s => _db.NamedEntity.Where(t => s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty 
                                && !TopicHelper.TopicBlackList.Contains(t.LemmaValue)))
                .AsEnumerable()
                .GroupBy(n => n.Value)
                .Select(g1 => new
                {
                    Type = g1.Key,
                    Count = g1.Count(),
                    Value = g1
                    .GroupBy(n => n.LemmaValue?.ToLower())
                    .Take(limit)
                    .Select(g2 => new { Value = g2.Key, Count = g2.Count() })
                    .OrderByDescending(n => n.Count)
                })
                .Take(limit)
                .ToList();
        }

        /// <summary>
        /// Fetches data for a chart which has the given ne as the first element and the rest ordered by descending
        /// to compare them
        /// </summary>
        /// <returns></returns>
        public dynamic GetNamedEntityComparedToOtherNamedEntities(
            int limit,
            string fraction,
            string party,
            string deputyId,
            DateTime from,
            DateTime to,
            string neLemmaValue)
        {
            if (fraction != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && _db.Deputies.SingleOrDefault(d => d.SpeakerId == s.SpeakerId).Fraction == fraction))
                .SelectMany(s => _db.NamedEntity.Where(t => s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty
                                && !TopicHelper.TopicBlackList.Contains(t.LemmaValue)))
                .AsEnumerable()
                .GroupBy(n => n.LemmaValue)
                .Select(kv => new
                {
                    Element = kv.Key,
                    Count = kv.Count()
                })
                .OrderBy(n => n.Element != neLemmaValue)
                .ThenByDescending(n => n.Count)
                .Take(limit)
                .ToList();
            else if(party != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && _db.Deputies.SingleOrDefault(d => d.SpeakerId == s.SpeakerId).Party == party))
                .SelectMany(s => _db.NamedEntity.Where(t => s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty
                                && !TopicHelper.TopicBlackList.Contains(t.LemmaValue)))
                .AsEnumerable()
                .GroupBy(n => n.LemmaValue)
                .Select(kv => new
                {
                    Element = kv.Key,
                    Count = kv.Count()
                })
                .OrderBy(n => n.Element != neLemmaValue)
                .ThenByDescending(n => n.Count)
                .Take(limit)
                .ToList();
            else if (deputyId != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                            && _db.Deputies.SingleOrDefault(d => d.SpeakerId == s.SpeakerId).Id.ToString() == deputyId))
                .SelectMany(s => _db.NamedEntity.Where(t => s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty
                                && !TopicHelper.TopicBlackList.Contains(t.LemmaValue)))
                .AsEnumerable()
                .GroupBy(n => n.LemmaValue)
                .Select(kv => new
                {
                    Element = kv.Key,
                    Count = kv.Count()
                })
                .OrderBy(n => n.Element != neLemmaValue)
                .ThenByDescending(n => n.Count)
                .Take(limit)
                .ToList();
            else
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod))
                .SelectMany(s => _db.NamedEntity.Where(t => s.Id == t.NLPSpeechId && t.ShoutId == Guid.Empty
                                && !TopicHelper.TopicBlackList.Contains(t.LemmaValue)))
                .AsEnumerable()
                .GroupBy(n => n.LemmaValue)
                .Select(kv => new
                {
                    Element = kv.Key,
                    Count = kv.Count()
                })
                .OrderBy(n => n.Element != neLemmaValue)
                .ThenByDescending(n => n.Count)
                .Take(limit)
                .ToList();
        }

        /// <summary>
        /// Gets the comments of speeches which contain the given topic by the given speaker, fraction, party.
        /// </summary>
        /// <returns></returns>
        public List<SpeechCommentViewModel> GetCommentsAboutTopic(
            int limit,
            string fraction,
            string party,
            string deputyId,
            DateTime from,
            DateTime to,
            string neLemmaValue)
        {
            if (fraction != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches.Include(s => s.Segments).ThenInclude(s => s.Shouts)
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                        && _db.NamedEntity.Any(ne => ne.NLPSpeechId == s.Id && ne.LemmaValue == neLemmaValue)
                        && s.Segments.Any(se => se.Text.Contains(neLemmaValue) && se.Shouts
                            .Any(sh => _db.Deputies.FirstOrDefault(d => d.SpeakerId == sh.SpeakerId && d.Fraction == fraction) != null))))
                .Take(limit)
                .AsEnumerable()
                .SelectMany(speech =>
                {
                    var speechSegment = speech.Segments
                        .Where(ss => ss.Text.Contains(neLemmaValue) && ss.Shouts
                            .Any(sh => _db.Deputies.FirstOrDefault(d => d.SpeakerId == sh.SpeakerId && d.Fraction == fraction) != null));

                    var speaker = _db.Deputies.FirstOrDefault(d => d.SpeakerId == speech.SpeakerId);

                    return speechSegment.Where(ss => ss != null).Select(ss => new SpeechCommentViewModel()
                    {
                        Speaker = speaker,
                        SpeechId = speech.Id,
                        SpeechSegment = ss
                    });
                })
                .Take(10)
                .ToList();
            else if (party != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches.Include(s => s.Segments).ThenInclude(s => s.Shouts)
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                        && _db.NamedEntity.Any(ne => ne.NLPSpeechId == s.Id && ne.LemmaValue == neLemmaValue)
                        && s.Segments.Any(se => se.Text.Contains(neLemmaValue) && se.Shouts
                            .Any(sh => _db.Deputies.FirstOrDefault(d => d.SpeakerId == sh.SpeakerId && d.Party == party) != null))))
                .Take(limit)
                .AsEnumerable()
                .SelectMany(speech =>
                {
                    var speechSegment = speech.Segments
                        .Where(ss => ss.Text.Contains(neLemmaValue) && ss.Shouts
                            .Any(sh => _db.Deputies.FirstOrDefault(d => d.SpeakerId == sh.SpeakerId && d.Party == party) != null));

                    var speaker = _db.Deputies.FirstOrDefault(d => d.SpeakerId == speech.SpeakerId);

                    return speechSegment.Where(ss => ss != null).Select(ss => new SpeechCommentViewModel()
                    {
                        Speaker = speaker,
                        SpeechId = speech.Id,
                        SpeechSegment = ss
                    });
                })
                .Take(10)
                .ToList();
            else if (deputyId != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches.Include(s => s.Segments).ThenInclude(s => s.Shouts)
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                        && _db.NamedEntity.Any(ne => ne.NLPSpeechId == s.Id && ne.LemmaValue == neLemmaValue)
                        && s.Segments.Any(se => se.Text.Contains(neLemmaValue) && se.Shouts
                            .Any(sh => _db.Deputies.FirstOrDefault(d => d.SpeakerId == sh.SpeakerId && d.Id.ToString() == deputyId) != null))))
                .Take(limit)
                .AsEnumerable()
                .SelectMany(speech =>
                {
                    var speechSegment = speech.Segments
                        .Where(ss => ss.Text.Contains(neLemmaValue) && ss.Shouts
                            .Any(sh => _db.Deputies.FirstOrDefault(d => d.SpeakerId == sh.SpeakerId && d.Id.ToString() == deputyId) != null));

                    var speaker = _db.Deputies.FirstOrDefault(d => d.SpeakerId == speech.SpeakerId);

                    return speechSegment.Where(ss => ss != null).Select(ss => new SpeechCommentViewModel()
                    {
                        Speaker = speaker,
                        SpeechId = speech.Id,
                        SpeechSegment = ss
                    });
                })
                .Take(10)
                .ToList();
            else
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches.Include(s => s.Segments).ThenInclude(s => s.Shouts)
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                        && _db.NamedEntity.Any(ne => ne.NLPSpeechId == s.Id && ne.LemmaValue == neLemmaValue)))
                .Take(limit)
                .AsEnumerable()
                .SelectMany(speech =>
                {
                    var speechSegment = speech.Segments.Where(ss => ss.Text.Contains(neLemmaValue));

                    var speaker = _db.Deputies.FirstOrDefault(d => d.SpeakerId == speech.SpeakerId);

                    return speechSegment.Where(ss => ss != null).Select(ss => new SpeechCommentViewModel()
                    {
                        Speaker = speaker,
                        SpeechId = speech.Id,
                        SpeechSegment = ss
                    });
                })
                .Take(10)
                .ToList();
        }

        /// <summary>
        /// Returns all speeches that contain the given ne
        /// </summary>
        /// <returns></returns>
        public List<SpeechViewModel> GetSpeechesOfNamedEnitity(
            int limit,
            string fraction,
            string party,
            string deputyId,
            DateTime from,
            DateTime to,
            string neLemmaValue)
        {
            if (fraction != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                        && _db.Deputies.SingleOrDefault(d => d.SpeakerId == s.SpeakerId).Fraction == fraction
                        && _db.NamedEntity.Any(ne => ne.NLPSpeechId == s.Id && ne.LemmaValue == neLemmaValue && ne.ShoutId == Guid.Empty)))
                .Select(s => new SpeechViewModel()
                {
                    Speech = s,
                    Agenda = _metadataService.GetAgendaItemOfSpeech(s),
                    TopicMentionCount = _db.NamedEntity
                        .Where(ne => ne.NLPSpeechId == s.Id && ne.LemmaValue == neLemmaValue).Count(),
                    Speaker = _db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId)
                })
                .OrderByDescending(s => s.TopicMentionCount)
                .Take(limit)
                .ToList();
            else if (party != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                        && _db.Deputies.SingleOrDefault(d => d.SpeakerId == s.SpeakerId).Party == party
                        && _db.NamedEntity.Any(ne => ne.NLPSpeechId == s.Id && ne.LemmaValue == neLemmaValue && ne.ShoutId == Guid.Empty)))
                .Select(s => new SpeechViewModel()
                {
                    Speech = s,
                    Agenda = _metadataService.GetAgendaItemOfSpeech(s),
                    TopicMentionCount = _db.NamedEntity
                        .Where(ne => ne.NLPSpeechId == s.Id && ne.LemmaValue == neLemmaValue).Count(),
                    Speaker = _db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId)
                })
                .OrderByDescending(s => s.TopicMentionCount)
                .Take(limit)
                .ToList();
            else if (deputyId != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                        && _db.Deputies.SingleOrDefault(d => d.SpeakerId == s.SpeakerId).Id.ToString() == deputyId
                        && _db.NamedEntity.Any(ne => ne.NLPSpeechId == s.Id && ne.LemmaValue == neLemmaValue && ne.ShoutId == Guid.Empty)))
                .Select(s => new SpeechViewModel()
                {
                    Speech = s,
                    Agenda = _metadataService.GetAgendaItemOfSpeech(s),
                    TopicMentionCount = _db.NamedEntity
                        .Where(ne => ne.NLPSpeechId == s.Id && ne.LemmaValue == neLemmaValue).Count(),
                    Speaker = _db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId)
                })
                .OrderByDescending(s => s.TopicMentionCount)
                .Take(limit)
                .ToList();
            else
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                    .SelectMany(p => _db.Speeches
                        .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                        && _db.NamedEntity.Any(ne => ne.NLPSpeechId == s.Id && ne.LemmaValue == neLemmaValue && ne.ShoutId == Guid.Empty)))
                    .Select(s => new SpeechViewModel()
                    {
                        Speech = s,
                        Agenda = _metadataService.GetAgendaItemOfSpeech(s),
                        TopicMentionCount = _db.NamedEntity
                            .Where(ne => ne.NLPSpeechId == s.Id && ne.LemmaValue == neLemmaValue).Count(),
                        Speaker = _db.Deputies.FirstOrDefault(d => d.SpeakerId == s.SpeakerId)
                    })
                    .OrderByDescending(s => s.TopicMentionCount)
                    .Take(limit)
                    .ToList();
        }

        /// <summary>
        /// Fetches a named entity by a name, fraction, party, speaker and so on and looks through all sentiments.
        /// </summary>
        /// <returns></returns>
        public dynamic GetNamedEntityWithCorrespondingSentiment(
            string searchTerm,
            DateTime from,
            DateTime to,
            string fraction,
            string party,
            string speakerId)
        {
            if (fraction != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                        && _db.Deputies.SingleOrDefault(d => d.SpeakerId == s.SpeakerId).Fraction == fraction))
                .SelectMany(s => _db.NamedEntity
                    .Where(n => s.Id == n.NLPSpeechId && n.LemmaValue == searchTerm && n.ShoutId == Guid.Empty))
                .AsEnumerable()
                .Select(ne =>
                {
                    var sentiment = _db.Sentiment.SingleOrDefault(s => s.NLPSpeechId == ne.NLPSpeechId && s.ShoutId == Guid.Empty && s.Begin <= ne.Begin && s.End >= ne.End);
                    if (sentiment == null) return null;
                    var value = "neu";
                    if (sentiment.SentimentSingleScore > 0) value = "pos";
                    else if (sentiment.SentimentSingleScore < 0) value = "neg";
                    return new
                    {
                        SentimentStringScore = value
                    };
                })
                .GroupBy(e => e?.SentimentStringScore)
                .Select(e => new
                {
                    Count = e.Count(),
                    Value = e.Key
                })
                .ToList();
            else if (party != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                        && _db.Deputies.SingleOrDefault(d => d.SpeakerId == s.SpeakerId).Party == party))
                .SelectMany(s => _db.NamedEntity
                    .Where(n => s.Id == n.NLPSpeechId && n.LemmaValue == searchTerm && n.ShoutId == Guid.Empty))
                .AsEnumerable()
                .Select(ne =>
                {
                    var sentiment = _db.Sentiment.SingleOrDefault(s => s.NLPSpeechId == ne.NLPSpeechId && s.ShoutId == Guid.Empty && s.Begin <= ne.Begin && s.End >= ne.End);
                    if (sentiment == null) return null;
                    var value = "neu";
                    if (sentiment.SentimentSingleScore > 0) value = "pos";
                    else if (sentiment.SentimentSingleScore < 0) value = "neg";
                    return new
                    {
                        SentimentStringScore = value
                    };
                })
                .GroupBy(e => e?.SentimentStringScore)
                .Select(e => new
                {
                    Count = e.Count(),
                    Value = e.Key
                })
                .ToList();
            else if (speakerId != string.Empty)
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod
                        && s.SpeakerId == speakerId))
                .SelectMany(s => _db.NamedEntity
                    .Where(n => s.Id == n.NLPSpeechId && n.LemmaValue == searchTerm && n.ShoutId == Guid.Empty))
                .AsEnumerable()
                .Select(ne =>
                {
                    var sentiment = _db.Sentiment.SingleOrDefault(s => s.NLPSpeechId == ne.NLPSpeechId && s.ShoutId == Guid.Empty && s.Begin <= ne.Begin && s.End >= ne.End);
                    if (sentiment == null) return null;
                    var value = "neu";
                    if (sentiment.SentimentSingleScore > 0) value = "pos";
                    else if (sentiment.SentimentSingleScore < 0) value = "neg";
                    return new
                    {
                        SentimentStringScore = value
                    };
                })
                .GroupBy(e => e?.SentimentStringScore)
                .Select(e => new
                {
                    Count = e.Count(),
                    Value = e.Key
                })
                .ToList();
            else
                return _db.Protocols.Where(p => p.Date >= from && p.Date <= to)
                .SelectMany(p => _db.Speeches
                    .Where(s => s.ProtocolNumber == p.Number && s.LegislaturePeriod == p.LegislaturePeriod))
                .SelectMany(s => _db.NamedEntity
                    .Where(n => s.Id == n.NLPSpeechId && n.LemmaValue == searchTerm && n.ShoutId == Guid.Empty))
                .AsEnumerable()
                .Select(ne =>
                {
                    var sentiment = _db.Sentiment.SingleOrDefault(s => s.NLPSpeechId == ne.NLPSpeechId && s.ShoutId == Guid.Empty && s.Begin <= ne.Begin && s.End >= ne.End);
                    if (sentiment == null) return null;
                    var value = "neu";
                    if (sentiment.SentimentSingleScore > 0) value = "pos";
                    else if (sentiment.SentimentSingleScore < 0) value = "neg";
                    return new
                    {
                        SentimentStringScore = value
                    };
                })
                .GroupBy(e => e?.SentimentStringScore)
                .Select(e => new
                {
                    Count = e.Count(),
                    Value = e.Key
                })
                .ToList();
        }
    }
}
