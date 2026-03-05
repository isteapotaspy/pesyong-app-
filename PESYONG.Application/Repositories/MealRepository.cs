using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Infrastructure;

/// <summary>
/// This serves as the quasi-controller from an frontend-based API.
/// An ASP.NET backend wasn't implemented. Instead, the data doesn't 
/// go through an API layer and goes directly to the database. 
/// 
/// Speficially, this handles the meals in the system. 
/// </summary>

namespace PESYONG.ApplicationLogic.Repositories;

public class MealRepository
{
    private readonly AppDbContext _context;

    public MealRepository(AppDbContext context)
    {
        _context = context;
    }

    
    /// <summary>
    /// Creates a meal in the database.
    /// </summary>
    /// <param name="meal"></param>
    /// <returns></returns>
    public async Task CreateMealAsync(Meal meal)
    {
        // Add exception handling for colliding meal ID
        _context.Meals.Add(meal);
        Debug.Write($"\n\n The meal has ID of {meal.MealID} and is named {meal.MealName} \n\n");
        await _context.SaveChangesAsync();

    }

    /// <summary>
    /// Creates a meal in the database and returns the created meal with its assigned MealID.
    /// </summary>
    /// <param name="meal"></param>
    /// <returns></returns>
    public async Task<Meal> CreateMealAsyncReturnSelf(Meal meal)
    {
        _context.Meals.Add(meal);
        await _context.SaveChangesAsync();


        Debug.Write($"\n\n The meal has ID of {meal.MealID} and is named {meal.MealName} \n\n");
        // Check if this is valid
        return meal;
    }

    /// <summary>
    /// Grabs a meal by its ID. Returns null if not found.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Meal> GetMealByIdAsync(int id)
    {
        try
        {
            // Try without including navigation properties first
            return await _context.Meals
                .AsNoTracking() // Avoid change tracking issues
                .FirstOrDefaultAsync(m => m.MealID == id);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetMealByIdAsync error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Returns all the meals in the table.
    /// </summary>
    /// <returns></returns>
    public async Task<List<Meal>> GetAllMealsAsync()
    {
        var meals = await _context.Meals.ToListAsync();

        if (meals == null || !meals.Any())
        {
            Debug.WriteLine("No meals found in database");
            return new List<Meal>(); // Return empty list instead of null
        }

        Debug.WriteLine($"Retrieved {meals.Count} meals from database");
        return meals;
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
    public async Task<List<Meal>> GetMealsAsync(IQueryable<Meal> query)
    {
        return await query.ToListAsync();
    }

    /// <summary>
    /// Updates a meal. Pass through the entire meal object and update by its delta.
    /// </summary>
    /// <param name="meal"></param>
    /// <returns></returns>
    public async Task UpdateMealAsync(Meal meal)
    {
        _context.Meals.Update(meal);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a meal in the database.
    /// </summary>
    /// <param name="mealId"></param>
    /// <returns></returns>
    public async Task DeleteMealAsync(int mealId)
    {
        var meal = await _context.Meals.FindAsync(mealId);
        if (meal != null)
        {
            _context.Meals.Remove(meal);
            await _context.SaveChangesAsync();
        }
    }

}
