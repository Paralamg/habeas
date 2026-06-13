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

        builder.Property(u => u.DateOfBirth)
            .HasConversion(dob => dob!.Value, value => DateOfBirth.FromTrusted(value))
            .HasColumnType("date");

        builder.OwnsMany(u => u.Measurements, measurements =>
        {
            measurements.ToTable("body_measurements");
            measurements.WithOwner().HasForeignKey("user_id");

            measurements.HasKey(m => m.Id);
            measurements.Property(m => m.Id)
                .HasConversion(id => id.Value, value => new BodyMeasurementId(value))
                .ValueGeneratedNever();

            measurements.Property(m => m.MetricType)
                .HasConversion(type => type.Key, key => MetricType.FromKey(key)!)
                .HasColumnName("metric")
                .HasMaxLength(32)
                .IsRequired();

            measurements.Property(m => m.Value);
            measurements.Property(m => m.RecordedAt);
        });

        builder.Navigation(u => u.Measurements)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(u => u.CurrentBmi);
        builder.Ignore(u => u.DomainEvents);
    }
}
