namespace HiLoGame.Infrastructure.Persistence.Configurations;

using Domain.Aggregates.Room.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public sealed class RoomPlayerConfig : IEntityTypeConfiguration<RoomPlayer>
{
    public void Configure(EntityTypeBuilder<RoomPlayer> b)
    {
        b.ToTable("RoomPlayers");

        // Clé composite (RoomId + PlayerId)
        b.HasKey(rp => new { rp.RoomId, rp.PlayerId });

        b.Property(rp => rp.RoomId)
            .ValueGeneratedNever();

        b.Property(rp => rp.PlayerId)
            .IsRequired()
            .HasMaxLength(450);

        b.Property(rp => rp.PlayerUsername)
            .IsRequired()
            .HasMaxLength(255);

        b.Property(rp => rp.Status)
            .HasConversion<int>() // stocke l'enum en int
            .IsRequired();

        b.Property(rp => rp.Attempts)
            .HasDefaultValue(0);

        
        b.Property(rp => rp.JoinedAt)
            .HasConversion(
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        // --- Converters UTC (nullable & non-nullable) ---
        var utc = new ValueConverter<DateTime, DateTime>(
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var utcNullable = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        b.Property(rp => rp.JoinedAt)
            .HasConversion(utc);

        b.Property(rp => rp.FirstGuessAt)
            .HasConversion(utcNullable);

        b.Property(rp => rp.LastGuessAt)
            .HasConversion(utcNullable);

        b.Property(rp => rp.SecretIsGreaterThan)
            .HasDefaultValue(int.MinValue);

        b.Property(rp => rp.SecretIsLessThan)
            .HasDefaultValue(int.MaxValue);


        // Index pratique pour requêter les membres d'une room par ordre d'arrivée
        b.HasIndex(rp => new { rp.RoomId, rp.JoinedAt });



    }
}