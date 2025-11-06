using Microsoft.EntityFrameworkCore;
using RewardStart.Core.Models;
using System.Reflection;

namespace RewardStart.Core;

public class RewardStartDbContext : DbContext
{
    public RewardStartDbContext(DbContextOptions<RewardStartDbContext> options)
        : base(options)
    {
    }

    public string DBPath { get; private set; }

    public DbSet<Activity> Activities { get; set; }

    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DBPath = Path.Combine(path, "RewardStar.db");
            optionsBuilder.UseSqlite($"Data Source={DBPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}