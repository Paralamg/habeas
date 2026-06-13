using Habeas.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Habeas.Infrastructure.Persistence;

/// <summary>
/// The single EF Core context for the app. Entity-to-table mapping lives in the
/// per-aggregate <c>IEntityTypeConfiguration</c> classes, applied below.
/// </summary>
public sealed class HabeasDbContext(DbContextOptions<HabeasDbContext> options) : DbContext(options)
{
    public DbSet<UserProfile> Users => Set<UserProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HabeasDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
