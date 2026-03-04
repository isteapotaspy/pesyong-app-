using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Entities.Audits;
using PESYONG.Domain.Entities.Financial;
using PESYONG.Domain.Entities.Financial.AcknowledgementReceipts;
using PESYONG.Domain.Entities.Financial.Promos;
using PESYONG.Domain.Entities.Logistics;
using PESYONG.Domain.Entities.Users.Identity;

namespace PESYONG.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseInMemoryDatabase("TestDatabase"); // FIXED: removed underscore
        }
        base.OnConfiguring(optionsBuilder);
    }

    // All DbSets
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AcknowledgementReceipt> AcknowledgementReceipts => Set<AcknowledgementReceipt>();
    public DbSet<Promo> Promos => Set<Promo>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<DeliveryUpdate> DeliveryUpdates => Set<DeliveryUpdate>();
    public DbSet<OrderMealProduct> OrderMealProducts => Set<OrderMealProduct>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Meal configurations
        modelBuilder.Entity<Meal>(entity =>
        {
            entity.HasKey(e => e.MealID);
            entity.Property(m => m.MealPrice).HasPrecision(18, 2);
        });

        // Order configurations
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderID);
            entity.HasOne(e => e.Recipient)
                  .WithMany()
                  .HasForeignKey(e => e.RecipientID)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Receipt)
                  .WithMany()
                  .HasForeignKey(e => e.ReceiptID)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // OrderMealProduct configuration
        modelBuilder.Entity<OrderMealProduct>(entity =>
        {
            entity.HasKey(e => new { e.OrderID, e.MealProductID });
            entity.HasOne(e => e.Order)
                  .WithMany(e => e.OrderItems)
                  .HasForeignKey(e => e.OrderID);
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogID);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserID)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // AcknowledgementReceipt configuration
        modelBuilder.Entity<AcknowledgementReceipt>(entity =>
        {
            entity.HasKey(e => e.AcknowledgementReceiptID);
            entity.HasOne(e => e.Order)
                  .WithMany()
                  .HasForeignKey(e => e.OrderID);
            entity.HasOne(e => e.Customer)
                  .WithMany()
                  .HasForeignKey(e => e.CustomerID);
        });

        // Promo configuration
        modelBuilder.Entity<Promo>(entity =>
        {
            entity.HasKey(e => e.PromoID);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // Payment configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentID);
            entity.HasOne(e => e.Receipt)
                  .WithMany()
                  .HasForeignKey(e => e.AcknowledgementRecieptID);
        });

        // Delivery configuration
        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasKey(e => e.DeliveryID);
            entity.HasOne(e => e.Order)
                  .WithMany()
                  .HasForeignKey(e => e.OrderID);
            entity.HasOne(e => e.DeliveryPersonnel)
                  .WithMany()
                  .HasForeignKey(e => e.DeliveryPersonnelID)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // DeliveryUpdate configuration
        modelBuilder.Entity<DeliveryUpdate>(entity =>
        {
            entity.HasKey(e => e.DeliveryUpdateID);
            entity.HasOne(e => e.Delivery)
                  .WithMany(e => e.DeliveryUpdates)
                  .HasForeignKey(e => e.DeliveryID);
        });
    }
}
