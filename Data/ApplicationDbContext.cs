using LastBiteNew.Data;
using LastBiteNew.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Restaurant> Restaurants { get; set; }
    public DbSet<FoodPackage> FoodPackages { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);


        // One-to-one: ApplicationUser ↔ Restaurant
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Restaurant)
            .WithOne(r => r.Owner)
            .HasForeignKey<Restaurant>(r => r.OwnerId);

        // One restaurant per owner (unique index)
        builder.Entity<Restaurant>()
            .HasIndex(r => r.OwnerId)
            .IsUnique();

       

        // One reservation per customer per package
        builder.Entity<Reservation>()
            .HasIndex(r => new { r.PackageId, r.CustomerId })
            .IsUnique();

        // One review per reservation
        builder.Entity<Review>()
            .HasIndex(r => r.ReservationId)
            .IsUnique();

        // Unique reservation code
        builder.Entity<Reservation>()
            .HasIndex(r => r.ReservationCode)
            .IsUnique();

        // Performance indexes
        builder.Entity<FoodPackage>()
            .HasIndex(p => p.Status);

        builder.Entity<FoodPackage>()
            .HasIndex(p => p.PickupEndTime);

        builder.Entity<FoodPackage>()
            .HasIndex(p => new { p.RestaurantId, p.Status });

        builder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.IsRead });

        builder.Entity<Restaurant>()
            .HasIndex(r => r.Status);

        builder.Entity<Restaurant>()
            .HasIndex(r => r.City);

        // CRITICAL: prevents cascade delete cycle — migration WILL fail without these

        builder.Entity<Review>()
            .HasOne(r => r.Customer)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Reservation>()
            .HasOne(r => r.Customer)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Map Reservation.FoodPackage to the existing PackageId FK
        // (otherwise EF creates a shadow "FoodPackagePackageId" column).
        builder.Entity<Reservation>()
            .HasOne(r => r.FoodPackage)
            .WithMany(p => p.Reservations)
            .HasForeignKey(r => r.PackageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Also restrict Review → Restaurant to break the cycle
        builder.Entity<Review>()
            .HasOne(r => r.Restaurant)
            .WithMany(r => r.Reviews)
            .HasForeignKey(r => r.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}