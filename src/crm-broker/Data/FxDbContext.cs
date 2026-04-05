using FxWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FxWebApi.Data;

public class FxDbContext : DbContext
{
    public FxDbContext(DbContextOptions<FxDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerPortfolio> CustomerPortfolios => Set<CustomerPortfolio>();
    public DbSet<CustomerHistory> CustomerHistories => Set<CustomerHistory>();
    public DbSet<CustomerPreference> CustomerPreferences => Set<CustomerPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
    }
}
