using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.Models.Database.MongoDB
{
    public class Token : DBEntity
    {
        public Guid NLPSpeechId { get; set; }
        public Guid ShoutId { get; set; }
        public int Begin { get; set; }
        public int End { get; set; }
        public string LemmaValue { get; set; }
        public string Stem { get; set; }
        public string posValue { get; set; }
        public bool Morph { get; set; }
        public string Gender { get; set; }
        public string Number { get; set; }
        public string Case { get; set; }
        public string Degree { get; set; }
        public string Value { get; set; }
        public string Aspect { get; set; }
        public string VerbForm { get; set; }
        public string Tense { get; set; }
        public string Mood { get; set; }
        public string Voice { get; set; }
        public string Definiteness { get; set; }
        public string Person { get; set; }
        public string Animacy { get; set; }
        public string Negative { get; set; }
        public string Possessive { get; set; }
        public string PronType { get; set; }
        public string Reflex { get; set; }
        public string Transitivity { get; set; }
    }
}
