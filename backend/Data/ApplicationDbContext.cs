using BiteNest.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BiteNest.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Rider> Riders => Set<Rider>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<MealPlan> MealPlans => Set<MealPlan>();
    public DbSet<LeftoverFoodOffer> LeftoverFoodOffers => Set<LeftoverFoodOffer>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Convert enums to strings for readable DB queries
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Rider>()
            .Property(r => r.CurrentStatus)
            .HasConversion<string>();

        modelBuilder.Entity<Rider>()
            .Property(r => r.VehicleType)
            .HasConversion<string>();

        modelBuilder.Entity<Payment>()
            .Property(p => p.Method)
            .HasConversion<string>();

        modelBuilder.Entity<Payment>()
            .Property(p => p.Status)
            .HasConversion<string>();

        modelBuilder.Entity<MealPlan>()
            .Property(m => m.TargetType)
            .HasConversion<string>();

        modelBuilder.Entity<Notification>()
            .Property(n => n.Type)
            .HasConversion<string>();

        // Relationships & Foreign Keys
        modelBuilder.Entity<Restaurant>()
            .HasOne(r => r.Owner)
            .WithMany(u => u.Restaurants)
            .HasForeignKey(r => r.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Rider>()
            .HasOne(r => r.User)
            .WithOne(u => u.RiderProfile)
            .HasForeignKey<Rider>(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MenuItem>()
            .HasOne(m => m.Restaurant)
            .WithMany(r => r.MenuItems)
            .HasForeignKey(m => m.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(u => u.CustomerOrders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Restaurant)
            .WithMany(r => r.Orders)
            .HasForeignKey(o => o.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Rider)
            .WithMany(r => r.Deliveries)
            .HasForeignKey(o => o.RiderId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.ParentCombinedOrder)
            .WithMany(o => o.ChildCombinedOrders)
            .HasForeignKey(o => o.ParentCombinedOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.MenuItem)
            .WithMany(m => m.OrderItems)
            .HasForeignKey(oi => oi.MenuItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Order)
            .WithOne(o => o.Payment)
            .HasForeignKey<Payment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Review>()
            .HasOne(rv => rv.Customer)
            .WithMany(u => u.Reviews)
            .HasForeignKey(rv => rv.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Review>()
            .HasOne(rv => rv.Restaurant)
            .WithMany(r => r.Reviews)
            .HasForeignKey(rv => rv.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LeftoverFoodOffer>()
            .HasOne(l => l.Restaurant)
            .WithMany(r => r.LeftoverOffers)
            .HasForeignKey(l => l.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LeftoverFoodOffer>()
            .HasOne(l => l.MenuItem)
            .WithMany(m => m.LeftoverOffers)
            .HasForeignKey(l => l.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MealPlan>()
            .HasOne(m => m.Customer)
            .WithMany(u => u.MealPlans)
            .HasForeignKey(m => m.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
