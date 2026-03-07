using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PESYONG.Domain.Entities.Audits;
using PESYONG.Domain.Entities.Financial;
using PESYONG.Domain.Entities.Financial.AcknowledgementReceipts;
using PESYONG.Domain.Entities.Financial.Promos;
using PESYONG.Domain.Entities.Logistics;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Meals.MealProduct;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PESYONG.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Meal> Meals => Set<Meal>();
        public DbSet<MealProduct> MealProducts => Set<MealProduct>();   
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<Promo> Promos => Set<Promo>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Delivery> Deliveries => Set<Delivery>();
        public DbSet<DeliveryUpdate> DeliveryUpdates => Set<DeliveryUpdate>();
        public DbSet<OrderMealProduct> OrderMealProducts => Set<OrderMealProduct>();
        public DbSet<AcknowledgementReceipt> AcknowledgementReceipts => Set<AcknowledgementReceipt>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -----------------------------
            // Meal
            // -----------------------------
            modelBuilder.Entity<Meal>(entity =>
            {
                entity.HasKey(e => e.MealID);

                entity.Property(e => e.MealPrice)
                      .HasPrecision(18, 2);

                entity.Property(e => e.MealTags)
                      .HasConversion(
                          v => string.Join(",", v),
                          v => string.IsNullOrWhiteSpace(v)
                              ? new List<string>()
                              : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                      .Metadata.SetValueComparer(
                          new ValueComparer<ICollection<string>>(
                              (c1, c2) =>
                                  (c1 ?? Array.Empty<string>()).SequenceEqual(c2 ?? Array.Empty<string>()),
                              c =>
                                  (c ?? Array.Empty<string>())
                                  .Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                              c =>
                                  (ICollection<string>)(c == null ? new List<string>() : c.ToList())
                          ));
            });

            // -----------------------------
            // Order
            // -----------------------------
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderID);

                entity.HasOne(e => e.Recipient)
                      .WithMany()
                      .HasForeignKey(e => e.RecipientID)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Receipt)
                      .WithOne(e => e.Order)
                      .HasForeignKey<AcknowledgementReceipt>(e => e.OrderID)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // -----------------------------
            // OrderMealProduct
            // -----------------------------
            modelBuilder.Entity<OrderMealProduct>(entity =>
            {
                entity.HasKey(e => new { e.OrderID, e.MealProductID });

                entity.HasOne(e => e.Order)
                      .WithMany(e => e.OrderItems)
                      .HasForeignKey(e => e.OrderID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // -----------------------------
            // AuditLog
            // -----------------------------
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.AuditLogID);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserID)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // -----------------------------
            // AcknowledgementReceipt
            // -----------------------------
            modelBuilder.Entity<AcknowledgementReceipt>(entity =>
            {
                entity.HasKey(e => e.AcknowledgementReceiptID);

                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerID)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.Property(e => e.Subtotal).HasPrecision(18, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
                entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
                entity.Property(e => e.ShippingCost).HasPrecision(18, 2);
                entity.Property(e => e.GrandTotal).HasPrecision(18, 2);
            });

            // -----------------------------
            // Promo
            // -----------------------------
            modelBuilder.Entity<Promo>(entity =>
            {
                entity.HasKey(e => e.PromoID);
                entity.HasIndex(e => e.Code).IsUnique();

                entity.Property(e => e.DiscountPercentageValue).HasPrecision(18, 2);
                entity.Property(e => e.MinimumOrderAmount).HasPrecision(18, 2);
            });

            // -----------------------------
            // Payment
            // -----------------------------
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentID);

                entity.HasOne(e => e.Receipt)
                      .WithMany()
                      .HasForeignKey(e => e.AcknowledgementRecieptID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // -----------------------------
            // Delivery
            // -----------------------------
            modelBuilder.Entity<Delivery>(entity =>
            {
                entity.HasKey(e => e.DeliveryID);

                entity.HasOne(e => e.Order)
                      .WithMany()
                      .HasForeignKey(e => e.OrderID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.DeliveryPersonnel)
                      .WithMany()
                      .HasForeignKey(e => e.DeliveryPersonnelID)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // -----------------------------
            // DeliveryUpdate
            // -----------------------------
            modelBuilder.Entity<DeliveryUpdate>(entity =>
            {
                entity.HasKey(e => e.DeliveryUpdateID);

                entity.HasOne(e => e.Delivery)
                      .WithMany(e => e.DeliveryUpdates)
                      .HasForeignKey(e => e.DeliveryID)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}