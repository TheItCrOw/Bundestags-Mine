using BundestagMine.Models.Database;
using BundestagMine.Models.Database.MongoDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BundestagMine.SqlDatabase
{
    public class BundestagMineDbContext : DbContext
    {
        public BundestagMineDbContext(DbContextOptions<BundestagMineDbContext> options) : base(options)
        {

        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer(ConfigManager.GetConnectionString(), builder =>
        //    {
        //        builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        //    });
        //    base.OnConfiguring(optionsBuilder);
        //}

        public DbSet<Protocol> Protocols { get; set; }
        public DbSet<Deputy> Deputies { get; set; }
        public DbSet<NetworkData> NetworkDatas { get; set; }
        public DbSet<CommentNetworkNode> CommentNetworkNode { get; set; }
        public DbSet<CommentNetworkLink> CommentNetworkLink { get; set; }
        public DbSet<Speech> Speeches { get; set; }
        public DbSet<Shout> Shouts { get; set; }
        public DbSet<NLPSpeech> NLPSpeeches { get; set; }
        public DbSet<Token> Token { get; set; }
        public DbSet<NamedEntity> NamedEntity { get; set; }
        public DbSet<CategoryCoveredTagged> CategoryCoveredTagged { get; set; }
        public DbSet<Sentiment> Sentiment { get; set; }
        public DbSet<SpeechSegment> SpeechSegment { get; set; }
        public DbSet<Poll> Polls { get; set; }
        public DbSet<PollEntry> PollEntries { get; set; }
        public DbSet<AgendaItem> AgendaItems { get; set; }
        public DbSet<ImportedEntity> ImportedEntities { get; set; }
        public DbSet<DailyPaper> DailyPapers { get; set; }
        public DbSet<DailyPaperSubscription> DailyPaperSubscriptions { get; set; }
        public DbSet<TextSummarizationEvaluationScore> TextSummarizationEvaluationScores { get; set; }
    }
}
