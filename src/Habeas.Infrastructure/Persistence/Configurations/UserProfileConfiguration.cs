using Habeas.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Habeas.Infrastructure.Persistence.Configurations;

internal sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(id => id.Value, value => new UserId(value))
            .ValueGeneratedNever();

        builder.Property(u => u.TelegramUserId)
            .HasConversion(id => id.Value, value => TelegramUserId.FromTrusted(value));
        builder.HasIndex(u => u.TelegramUserId).IsUnique();

        builder.Property(u => u.DisplayName).HasMaxLength(256).IsRequired();

        builder.OwnsOne(u => u.BodyMetrics, metrics =>
        {
            metrics.Property(m => m.HeightCm);
            metrics.Property(m => m.WeightKg);
            metrics.Ignore(m => m.Bmi);
        });

        builder.Ignore(u => u.DomainEvents);
    }
}
