using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HiLoGame.Domain.Aggregates.Player;
using HiLoGame.Domain.Aggregates.Room.Entities;

namespace HiLoGame.Infrastructure.Persistence;

public class HiLoGameDbContext : IdentityDbContext<Player, IdentityRole, string>
{
    public DbSet<Player> Players { get; set; }
    public DbSet<Room> Rooms { get; set; }

    public HiLoGameDbContext(DbContextOptions<HiLoGameDbContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder b) 
    {
        base.OnModelCreating(b);

        // Scanne et applique toutes les IEntityTypeConfiguration<> du projet Infrastructure
        b.ApplyConfigurationsFromAssembly(typeof(HiLoGameDbContext).Assembly);
    }
}