﻿// <auto-generated />
using System;
using BundestagMine.SqlDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BundestagMine.SqlDatabase.Migrations
{
    [DbContext(typeof(BundestagMineDbContext))]
    [Migration("20230228131323_AddedTextSummarizationEvaluation")]
    partial class AddedTextSummarizationEvaluation
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("BundestagMine.Models.Database.DailyPaper", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("JsonDataString")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("LegislaturePeriod")
                        .HasColumnType("int");

                    b.Property<DateTime>("ProtocolDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("ProtocolNumber")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("DailyPapers");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.DailyPaperSubscription", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Active")
                        .HasColumnType("bit");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("InitialSubscriptionDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("LastSendTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("LastSentDailyPaperId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("DailyPaperSubscriptions");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.ImportedEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ImportedDate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ModelJson")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ProtocolId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("ImportedEntities");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.AgendaItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AgendaItemNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.Property<Guid>("ProtocolId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("AgendaItems");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.CategoryCoveredTagged", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Begin")
                        .HasColumnType("int");

                    b.Property<int>("End")
                        .HasColumnType("int");

                    b.Property<Guid>("NLPSpeechId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("Score")
                        .HasColumnType("float");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("NLPSpeechId");

                    b.ToTable("CategoryCoveredTagged");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.CommentNetworkLink", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("NetworkDataId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("Sentiment")
                        .HasColumnType("float");

                    b.Property<string>("Source")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Target")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Value")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("NetworkDataId");

                    b.ToTable("CommentNetworkLink");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.CommentNetworkNode", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("NetworkDataId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Party")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("NetworkDataId");

                    b.ToTable("CommentNetworkNode");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.Deputy", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AcademicTitle")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("BirthDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DeathDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Fraction")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Gender")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("HistorySince")
                        .HasColumnType("datetime2");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MaritalStatus")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MongoId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Party")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Profession")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Religion")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SpeakerId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Deputies");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.NamedEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Begin")
                        .HasColumnType("int");

                    b.Property<int>("End")
                        .HasColumnType("int");

                    b.Property<string>("LemmaValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("NLPSpeechId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ShoutId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("NLPSpeechId");

                    b.ToTable("NamedEntity");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.NetworkData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("MongoId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("NetworkDatas");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.Protocol", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AgendaItemsCount")
                        .HasColumnType("int");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<int>("LegislaturePeriod")
                        .HasColumnType("int");

                    b.Property<string>("MongoId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Number")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Protocols");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.Sentiment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Begin")
                        .HasColumnType("int");

                    b.Property<int>("End")
                        .HasColumnType("int");

                    b.Property<Guid>("NLPSpeechId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("SentimentSingleScore")
                        .HasColumnType("float");

                    b.Property<Guid>("ShoutId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("NLPSpeechId");

                    b.ToTable("Sentiment");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.Shout", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Fraction")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Party")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SpeakerId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("SpeechSegmentId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SpeechSegmentId");

                    b.ToTable("Shouts");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.Speech", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AgendaItemNumber")
                        .HasColumnType("int");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("LegislaturePeriod")
                        .HasColumnType("int");

                    b.Property<string>("MongoId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ProtocolNumber")
                        .HasColumnType("int");

                    b.Property<string>("SpeakerId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Speeches");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Speech");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.SpeechSegment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("SpeechId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SpeechId");

                    b.ToTable("SpeechSegment");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.Token", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Animacy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Aspect")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Begin")
                        .HasColumnType("int");

                    b.Property<string>("Case")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Definiteness")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Degree")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("End")
                        .HasColumnType("int");

                    b.Property<string>("Gender")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LemmaValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Mood")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Morph")
                        .HasColumnType("bit");

                    b.Property<Guid>("NLPSpeechId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Negative")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Number")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Person")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Possessive")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PronType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Reflex")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ShoutId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Stem")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Tense")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Transitivity")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("VerbForm")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Voice")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("posValue")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("NLPSpeechId");

                    b.ToTable("Token");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.Poll", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<int>("LegislaturePeriod")
                        .HasColumnType("int");

                    b.Property<int>("PollNumber")
                        .HasColumnType("int");

                    b.Property<int>("ProtocolNumber")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Polls");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.PollEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Abstention")
                        .HasColumnType("bit");

                    b.Property<string>("Comment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Designation")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Fraction")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("No")
                        .HasColumnType("bit");

                    b.Property<bool>("NotSubmitted")
                        .HasColumnType("bit");

                    b.Property<bool>("NotValid")
                        .HasColumnType("bit");

                    b.Property<Guid>("PollId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Yes")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("PollId");

                    b.ToTable("PollEntries");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.TextSummarizationEvaluationScore", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AverageWordsPerSentence")
                        .HasColumnType("int");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("LevenstheinSimilaritiesOfSentences")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("NamedEntityDistance")
                        .HasColumnType("float");

                    b.Property<string>("ScoreExplanation")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("SpeechId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("SummaryCompressionRate")
                        .HasColumnType("float");

                    b.Property<int>("SummaryScore")
                        .HasColumnType("int");

                    b.Property<int>("TextSummarizationMethod")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("TextSummarizationEvaluationScores");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.NLPSpeech", b =>
                {
                    b.HasBaseType("BundestagMine.Models.Database.MongoDB.Speech");

                    b.Property<string>("AbstractSummary")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AbstractSummaryPEGASUS")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EnglishTranslationOfSpeech")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ExtractiveSummary")
                        .HasColumnType("nvarchar(max)");

                    b.HasDiscriminator().HasValue("NLPSpeech");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.CategoryCoveredTagged", b =>
                {
                    b.HasOne("BundestagMine.Models.Database.MongoDB.NLPSpeech", null)
                        .WithMany("CategoryCoveredTags")
                        .HasForeignKey("NLPSpeechId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.CommentNetworkLink", b =>
                {
                    b.HasOne("BundestagMine.Models.Database.MongoDB.NetworkData", null)
                        .WithMany("Links")
                        .HasForeignKey("NetworkDataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.CommentNetworkNode", b =>
                {
                    b.HasOne("BundestagMine.Models.Database.MongoDB.NetworkData", null)
                        .WithMany("Nodes")
                        .HasForeignKey("NetworkDataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.NamedEntity", b =>
                {
                    b.HasOne("BundestagMine.Models.Database.MongoDB.NLPSpeech", null)
                        .WithMany("NamedEntities")
                        .HasForeignKey("NLPSpeechId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.Sentiment", b =>
                {
                    b.HasOne("BundestagMine.Models.Database.MongoDB.NLPSpeech", null)
                        .WithMany("Sentiments")
                        .HasForeignKey("NLPSpeechId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.Shout", b =>
                {
                    b.HasOne("BundestagMine.Models.Database.MongoDB.SpeechSegment", null)
                        .WithMany("Shouts")
                        .HasForeignKey("SpeechSegmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.SpeechSegment", b =>
                {
                    b.HasOne("BundestagMine.Models.Database.MongoDB.Speech", null)
                        .WithMany("Segments")
                        .HasForeignKey("SpeechId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.Token", b =>
                {
                    b.HasOne("BundestagMine.Models.Database.MongoDB.NLPSpeech", null)
                        .WithMany("Tokens")
                        .HasForeignKey("NLPSpeechId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BundestagMine.Models.Database.PollEntry", b =>
                {
                    b.HasOne("BundestagMine.Models.Database.Poll", null)
                        .WithMany("Entries")
                        .HasForeignKey("PollId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.NetworkData", b =>
                {
                    b.Navigation("Links");

                    b.Navigation("Nodes");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.Speech", b =>
                {
                    b.Navigation("Segments");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.SpeechSegment", b =>
                {
                    b.Navigation("Shouts");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.Poll", b =>
                {
                    b.Navigation("Entries");
                });

            modelBuilder.Entity("BundestagMine.Models.Database.MongoDB.NLPSpeech", b =>
                {
                    b.Navigation("CategoryCoveredTags");

                    b.Navigation("NamedEntities");

                    b.Navigation("Sentiments");

                    b.Navigation("Tokens");
                });
#pragma warning restore 612, 618
        }
    }
}
