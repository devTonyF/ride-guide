using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RideGuide.Domain.Entities;

namespace RideGuide.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Region> Regions => Set<Region>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Rating> Ratings => Set<Rating>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Route>().HasQueryFilter(r => !r.IsDeleted);
        modelBuilder.Entity<Rating>().HasQueryFilter(r => !r.IsDeleted);

        modelBuilder.Entity<Route>()
            .HasOne<Region>()
            .WithMany()
            .HasForeignKey(r => r.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Route>()
            .HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(r => r.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Rating>()
            .HasOne<Route>()
            .WithMany()
            .HasForeignKey(r => r.RouteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Rating>()
            .HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Rating>()
            .HasIndex(r => new { r.RouteId, r.UserId })
            .IsUnique();
    }
}
