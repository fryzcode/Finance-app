using FinanceApp.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Api.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Balance> Balances => Set<Balance>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<NonExcludableNeed> NonExcludableNeeds => Set<NonExcludableNeed>();
    public DbSet<SavingsGoal> SavingsGoals => Set<SavingsGoal>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<BalanceHistory> BalanceHistory => Set<BalanceHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Salary).HasColumnType("numeric(18,2)");
            entity.Property(u => u.AdditionalEarnings).HasColumnType("numeric(18,2)");

            entity.HasOne(u => u.Balance)
                  .WithOne(b => b.User)
                  .HasForeignKey<Balance>(b => b.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.Transactions)
                  .WithOne(t => t.User)
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.NonExcludableNeeds)
                  .WithOne(n => n.User)
                  .HasForeignKey(n => n.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.SavingsGoals)
                  .WithOne(g => g.User)
                  .HasForeignKey(g => g.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasIndex(t => new { t.UserId, t.Date });
            entity.Property(t => t.Type).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<NonExcludableNeed>(entity =>
        {
            entity.HasIndex(n => new { n.UserId, n.Category });
        });

        modelBuilder.Entity<SavingsGoal>(entity =>
        {
            entity.HasIndex(g => new { g.UserId, g.GoalName });
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasIndex(p => new { p.UserId, p.Token });
            entity.HasOne(p => p.User)
                  .WithMany()
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BalanceHistory>(entity =>
        {
            entity.HasIndex(b => new { b.UserId, b.Date });
            entity.HasOne(b => b.User)
                  .WithMany()
                  .HasForeignKey(b => b.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}


