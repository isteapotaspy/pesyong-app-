using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Meals.MealProduct;
using PESYONG.Infrastructure;

namespace PESYONG.ApplicationLogic.Repositories;


public class MealProductRepository
{
    private readonly AppDbContext _context;

    public MealProductRepository(AppDbContext context)
    {
        _context = context;
    }


    /// <summary>
    /// Creates a meal in the database.
    /// </summary>
    /// <param name="mealProduct"></param>
    /// <returns></returns>
    public async Task CreateMealProductAsync(MealProduct mealProduct)
    {
        // Add exception handling for colliding meal ID
        _context.MealProducts.Add(mealProduct);
        Debug.Write($"\n\n The meal product has ID of {mealProduct.MealProductID} " +
            $"and is named {mealProduct.ProductName} \n\n");
        await _context.SaveChangesAsync();

    }

    /// <summary>
    /// This creates a meal product and returns itself after being recorded in the db.
    /// </summary>
    /// <param name="mealProduct"></param>
    /// <returns></returns>
    public async Task<MealProduct> CreateMealProductAsyncReturnSelf(MealProduct mealProduct)
    {
        _context.MealProducts.Add(mealProduct);
        await _context.SaveChangesAsync();
        Debug.Write($"\n\n The meal product has ID of {mealProduct.MealProductID} " +
            $"and is named {mealProduct.ProductName} \n\n");
        // Check if this is valid
        return mealProduct;
    }

    /// <summary>
    /// Grabs a singular MealProduct by its ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<MealProduct> GetMealByIdAsync(Guid id)
    {
        try
        {
            // Try without including navigation properties first
            return await _context.MealProducts
                .AsNoTracking() // Avoid change tracking issues
                .FirstOrDefaultAsync(m => m.MealProductID == id);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"\n\nGetMealProductByIdAsync error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// This returns all meal products in the database
    /// </summary>
    /// <returns></returns>
    public async Task<List<MealProduct>> GetAllMealProductsAsync()
    {
        var mealProducts = await _context.MealProducts.ToListAsync();

        if (mealProducts == null || !mealProducts.Any())
        {
            Debug.WriteLine("No meals found in database");
            return new List<MealProduct>(); // Return empty list instead of null
        }

        Debug.WriteLine($"Retrieved {mealProducts.Count} meals from database");
        return mealProducts;
    }

    // Know the IQueryable pattern. This is a more potent implemention
    // as opposed to blind searching by a single ID or all the items.
    // It allows you to feed it commands regarding to how it grabs and filters
    // data from the database. You can get all data from this, or none at all. 
    // If it returns empty array [], no items match the query parameters.
    // If it reutrns null, that's an error.

    /// EXAMPLE:
    /// var complexQuery = _context.Meals
    ///         .Where(m => m.StockQuantity > 0)
    ///         .Where(m => m.MealPrice < 50)
    ///         .OrderBy(m => m.MMealName);
    /// var results = await _mealRepository.GetMealsAsync(complexQuery);
    /// 

    /// <summary>
    /// Allows for complex query to occur.
    /// </summary>
    /// <param name="query"></param>
    public async Task<List<MealProduct>> GetMealProductsAsync(IQueryable<MealProduct> query)
    {
        return await query.ToListAsync();
    }

    /// <summary>
    /// This updates a meal product.
    /// </summary>
    /// <param name="mealProduct"></param>
    /// <returns></returns>
    public async Task UpdateMealAsync(MealProduct mealProduct)
    {
        _context.MealProducts.Update(mealProduct);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a meal product in the database.
    /// </summary>
    /// <param name="mealProductID"></param>
    /// <returns></returns>
    public async Task DeleteMealAsync(Guid mealProductID)
    {
        var mealProduct = await _context.MealProducts.FindAsync(mealProductID);
        if (mealProduct != null)
        {
            _context.MealProducts.Remove(mealProduct);
            await _context.SaveChangesAsync();
        }
    }

}
