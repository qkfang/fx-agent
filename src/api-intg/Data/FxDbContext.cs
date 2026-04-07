using FxIntegrationApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FxIntegrationApi.Data;

public class FxDbContext : DbContext
{
    public FxDbContext(DbContextOptions<FxDbContext> options) : base(options) { }

    public DbSet<ResearchArticle> ResearchArticles => Set<ResearchArticle>();
    public DbSet<ResearchDraft> ResearchDrafts => Set<ResearchDraft>();
    public DbSet<ResearchPattern> ResearchPatterns => Set<ResearchPattern>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerPortfolio> CustomerPortfolios => Set<CustomerPortfolio>();
    public DbSet<CustomerHistory> CustomerHistories => Set<CustomerHistory>();
    public DbSet<CustomerPreference> CustomerPreferences => Set<CustomerPreference>();
    public DbSet<Trader> Traders => Set<Trader>();
    public DbSet<TraderRecommendation> TraderRecommendations => Set<TraderRecommendation>();
    public DbSet<TraderNewsFeed> TraderNewsFeeds => Set<TraderNewsFeed>();
    public DbSet<TraderSuggestion> TraderSuggestions => Set<TraderSuggestion>();
    public DbSet<CustomerSuggestion> CustomerSuggestions => Set<CustomerSuggestion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ResearchArticle>(entity =>
        {
            entity.ToTable("ResearchArticles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Author).HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.Sentiment).HasMaxLength(50);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(300);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Company).HasMaxLength(300);
            entity.HasMany(e => e.Portfolios)
                  .WithOne(p => p.Customer)
                  .HasForeignKey(p => p.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerPortfolio>(entity =>
        {
            entity.ToTable("CustomerPortfolios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CurrencyPair).HasMaxLength(20);
            entity.Property(e => e.Direction).HasMaxLength(10);
            entity.Property(e => e.Amount).HasPrecision(18, 4);
            entity.Property(e => e.EntryRate).HasPrecision(18, 6);
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<CustomerHistory>(entity =>
        {
            entity.ToTable("CustomerHistories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CurrencyPair).HasMaxLength(20);
            entity.Property(e => e.Direction).HasMaxLength(10);
            entity.Property(e => e.Amount).HasPrecision(18, 4);
            entity.Property(e => e.EntryRate).HasPrecision(18, 6);
            entity.Property(e => e.ExitRate).HasPrecision(18, 6);
            entity.Property(e => e.PnL).HasPrecision(18, 4);
            entity.HasOne(e => e.Customer)
                  .WithMany()
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerPreference>(entity =>
        {
            entity.ToTable("CustomerPreferences");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PreferredCurrencyPairs).HasMaxLength(500);
            entity.Property(e => e.RiskTolerance).HasMaxLength(20);
            entity.Property(e => e.MaxPositionSize).HasPrecision(18, 4);
            entity.Property(e => e.StopLossPercent).HasPrecision(5, 2);
            entity.Property(e => e.TakeProfitPercent).HasPrecision(5, 2);
            entity.Property(e => e.TradingStyle).HasMaxLength(50);
            entity.Property(e => e.NotificationChannels).HasMaxLength(200);
            entity.HasOne(e => e.Customer)
                  .WithMany()
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Trader>(entity =>
        {
            entity.ToTable("Traders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(300);
            entity.Property(e => e.Desk).HasMaxLength(100);
            entity.Property(e => e.Specialization).HasMaxLength(200);
            entity.Property(e => e.Region).HasMaxLength(100);
            entity.HasMany(e => e.Recommendations)
                  .WithOne(r => r.Trader)
                  .HasForeignKey(r => r.TraderId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.NewsFeeds)
                  .WithOne(n => n.Trader)
                  .HasForeignKey(n => n.TraderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TraderRecommendation>(entity =>
        {
            entity.ToTable("TraderRecommendations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CurrencyPair).HasMaxLength(20);
            entity.Property(e => e.Direction).HasMaxLength(10);
            entity.Property(e => e.TargetRate).HasPrecision(18, 6);
            entity.Property(e => e.StopLoss).HasPrecision(18, 6);
            entity.Property(e => e.Confidence).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<TraderNewsFeed>(entity =>
        {
            entity.ToTable("TraderNewsFeeds");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Headline).HasMaxLength(500);
            entity.Property(e => e.Source).HasMaxLength(200);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CurrencyPairs).HasMaxLength(200);
            entity.Property(e => e.Sentiment).HasMaxLength(50);
        });

        modelBuilder.Entity<ResearchDraft>(entity =>
        {
            entity.ToTable("ResearchDrafts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.Author).HasMaxLength(200);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<ResearchPattern>(entity =>
        {
            entity.ToTable("ResearchPatterns");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CurrencyPair).HasMaxLength(20);
            entity.Property(e => e.PatternName).HasMaxLength(200);
            entity.Property(e => e.Timeframe).HasMaxLength(50);
            entity.Property(e => e.Direction).HasMaxLength(10);
            entity.Property(e => e.ConfidenceScore).HasPrecision(5, 2);
            entity.Property(e => e.DetectedBy).HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<CustomerSuggestion>(entity =>
        {
            entity.ToTable("CustomerSuggestions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(300);
            entity.Property(e => e.Company).HasMaxLength(300);
            entity.Property(e => e.CurrencyPair).HasMaxLength(20);
            entity.Property(e => e.Direction).HasMaxLength(10);
            entity.Property(e => e.Confidence).HasMaxLength(20);
            entity.Property(e => e.SuggestedBy).HasMaxLength(200);
        });

        modelBuilder.Entity<TraderSuggestion>(entity =>
        {
            entity.ToTable("TraderSuggestions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RelevanceScore).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.HasOne(e => e.Trader)
                  .WithMany(t => t.Suggestions)
                  .HasForeignKey(e => e.TraderId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Customer)
                  .WithMany()
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ResearchArticle)
                  .WithMany()
                  .HasForeignKey(e => e.ResearchArticleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
