using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Orders;
using System.Reflection.Emit;

namespace PESYONG.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    if (!optionsBuilder.IsConfigured)
    //    {
    //        // Replace with your actual SQL Server connection string
    //        //optionsBuilder.UseSqlServer("Server=SQL(localdb);Database=AppDB;User Id=your_user;Password=your_password;");
    //        optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect " +
    //            "Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
    //    }        
    //}

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            _ = optionsBuilder.UseInMemoryDatabase("TestDatabase");
        }
        base.OnConfiguring(optionsBuilder);
    }


    // Your existing Domain Models
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ensure your Decimal prices don't lose precision
        modelBuilder.Entity<Meal>()
            .Property(m => m.MealPrice)
            .HasPrecision(18, 2);
    }
}
