using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PESYONG.Infrastructure;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Your existing Domain Models
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ensure your Decimal prices don't lose precision in SQL
        modelBuilder.Entity<Meal>()
            .Property(m => m.MealPrice)
            .HasPrecision(18, 2);
    }
}