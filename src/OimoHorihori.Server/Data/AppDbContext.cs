using Microsoft.EntityFrameworkCore;
using OimoHorihori.Server.Models;

namespace OimoHorihori.Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();
    public DbSet<GameSave> GameSaves => Set<GameSave>();
    public DbSet<RankingRecord> RankingRecords => Set<RankingRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<UserAccount>().HasIndex(user => user.NormalizedUserName).IsUnique();
        modelBuilder.Entity<UserAccount>().Property(user => user.UserName).HasMaxLength(10);
        modelBuilder.Entity<UserAccount>().Property(user => user.NormalizedUserName).HasMaxLength(10);
        modelBuilder.Entity<AuthSession>().HasIndex(session => session.TokenHash).IsUnique();
        modelBuilder.Entity<AuthSession>().HasIndex(session => session.UserId);
        modelBuilder.Entity<GameSave>().HasKey(save => save.UserId);
        modelBuilder.Entity<RankingRecord>().HasKey(record => record.UserId);
        modelBuilder.Entity<RankingRecord>().Property(record => record.UserName).HasMaxLength(10);
        modelBuilder.Entity<RankingRecord>().HasIndex(record => record.TotalPotato);
        modelBuilder.Entity<RankingRecord>().HasIndex(record => record.BestProductionPerSecond);
        modelBuilder.Entity<RankingRecord>().HasIndex(record => record.ReplantCount);
        modelBuilder.Entity<UserAccount>().Property(user => user.EquippedTitleId).HasMaxLength(50);
    }
}