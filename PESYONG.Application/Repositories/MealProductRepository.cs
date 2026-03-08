using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Entities.Meals.MealProduct;
using PESYONG.Infrastructure;

namespace PESYONG.ApplicationLogic.Repositories;

public class MealProductRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public MealProductRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task CreateMealProductAsync(MealProduct mealProduct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.Set<MealProduct>().Add(mealProduct);
        Debug.WriteLine($"\n\nCreating meal product: {mealProduct.ProductName}\n\n");
        await context.SaveChangesAsync();
    }

    public async Task<MealProduct> CreateMealProductAsyncReturnSelf(MealProduct mealProduct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.Set<MealProduct>().Add(mealProduct);
        await context.SaveChangesAsync();

        Debug.WriteLine($"\n\nCreated meal product ID {mealProduct.MealProductID}: {mealProduct.ProductName}\n\n");
        return mealProduct;
    }

    public async Task<MealProduct?> GetMealProductByIdAsync(int id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Set<MealProduct>()
                .AsNoTracking()
                .Include(x => x.Owner)
                .Include(x => x.Promo)
                .Include(x => x.MealProductItems)
                    .ThenInclude(x => x.Meal)
                .FirstOrDefaultAsync(x => x.MealProductID == id);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetMealProductByIdAsync error: {ex}");
            return null;
        }
    }

    public async Task<List<MealProduct>> GetAllMealProductsAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var items = await context.Set<MealProduct>()
                .AsNoTracking()
                .Include(x => x.Owner)
                .Include(x => x.Promo)
                .Include(x => x.MealProductItems)
                    .ThenInclude(x => x.Meal)
                .ToListAsync();

            if (items == null || !items.Any())
            {
                Debug.WriteLine("No meal products found in database.");
                return new List<MealProduct>();
            }

            Debug.WriteLine($"Retrieved {items.Count} meal products from database.");
            return items;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetAllMealProductsAsync error: {ex}");
            return new List<MealProduct>();
        }
    }

    public async Task<List<MealProduct>> GetMealProductsAsync(
        Func<IQueryable<MealProduct>, IQueryable<MealProduct>> queryBuilder)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        IQueryable<MealProduct> query = context.Set<MealProduct>()
            .Include(x => x.Owner)
            .Include(x => x.Promo)
            .Include(x => x.MealProductItems)
                .ThenInclude(x => x.Meal);

        query = queryBuilder(query);

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task UpdateMealProductAsync(MealProduct mealProduct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.Set<MealProduct>().Update(mealProduct);
        await context.SaveChangesAsync();

        Debug.WriteLine($"Updated meal product ID {mealProduct.MealProductID}");
    }

    public async Task DeleteMealProductAsync(int mealProductId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var entity = await context.Set<MealProduct>()
            .Include(x => x.MealProductItems)
            .FirstOrDefaultAsync(x => x.MealProductID == mealProductId);

        if (entity == null)
        {
            Debug.WriteLine($"Delete skipped. Meal product ID {mealProductId} not found.");
            return;
        }

        context.Set<MealProduct>().Remove(entity);
        await context.SaveChangesAsync();

        Debug.WriteLine($"Deleted meal product ID {mealProductId}");
    }
}