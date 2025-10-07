using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using HiLoGame.Domain.Aggregates.Player;

namespace HiLoGame.Infrastructure.Persistence.Configurations;

public class PlayerConfig : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> e)
    {

        e.ToTable("AspNetUsers"); // keep the default Identity table name
        e.Property(x => x.RegisteredAt).IsRequired();

        e.Property(p => p.RefreshToken).HasMaxLength(512);
        e.Property(p => p.RefreshTokenExpiryTime);
    }
}