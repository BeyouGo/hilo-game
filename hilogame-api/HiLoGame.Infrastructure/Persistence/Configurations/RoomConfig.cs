using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using HiLoGame.Domain.Aggregates.Room.Entities;

namespace HiLoGame.Infrastructure.Persistence.Configurations;

public class RoomConfig : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> b)
    {

        b.ToTable("Rooms");
        b.HasKey(r => r.Id);

        b.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        b.OwnsOne(x => x.Rules, rules =>
        {
            rules.Property(p => p.Min).HasColumnName("Min").IsRequired();
            rules.Property(p => p.Max).HasColumnName("Max").IsRequired();
            rules.Property(p => p.MaxPlayers).HasColumnName("MaxPlayers").IsRequired();
        });

        b.Property(r => r.Secret).IsRequired();
        b.Property(r => r.Status)
            .HasConversion<int>()
            .IsRequired();

        b.Property(r => r.OwnerId)
            .IsRequired()
            .HasMaxLength(450); 

        b.Property(r => r.OwnerUsername)
            .IsRequired()
            .HasMaxLength(255);

        b.Property(r => r.WinnerPlayerId)
            .IsRequired(false)
            .HasMaxLength(450);

        b.Property(r => r.WinnerPlayerUsername)
            .IsRequired(false)
            .HasMaxLength(255);


        b.Property(r => r.CreatedAt)
            .HasConversion(
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        // Concurrency token (shadow)
        b.Property<byte[]>("RowVersion").IsRowVersion();



        // (Optionnel) Relation vers AspNetUsers si tu veux une contrainte FK
        // b.HasOne<Player>()
        //  .WithMany()
        //  .HasForeignKey(r => r.OwnerId)
        //  .OnDelete(DeleteBehavior.Restrict);

        // --- Collection avec backing field _players ---

        // Map the collection via the NAVIGATION (not the field)
        b.HasMany(r => r.Players)
            .WithOne(rp => rp.Room)
            .HasForeignKey(rp => rp.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Tell EF the backing field for Players is _players + use field access
        var playersNav = b.Metadata.FindNavigation(nameof(Room.Players))!;
        playersNav.SetField("_players");
        playersNav.SetPropertyAccessMode(PropertyAccessMode.Field);


        // Index utiles
        b.HasIndex(r => r.OwnerId);
        b.HasIndex(r => new { r.Status, r.CreatedAt });
    }
}