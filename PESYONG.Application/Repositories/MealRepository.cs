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

    // We should probably add an authorization logic to this
    // Depends on the app user logging in, probably
    // CREATE. To initialize a new object in the database.
    public async Task CreateMealAsync(Meal meal)
    {
        _context.Meals.Add(meal);
        await _context.SaveChangesAsync();
    }

    public async Task<Meal> GetMealByIdAsync(int id)
    {
        return await _context.Meals.FirstOrDefaultAsync(m => m.MealID == id);
    }

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
    public async Task<List<Meal>> GetMealsAsync(IQueryable<Meal> query)
    {
        return await query.ToListAsync();
    }

    public async Task UpdateMealAsync(Meal meal)
    {
        _context.Meals.Update(meal);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteMealAsync(int MealId)
    {
        var meal = await _context.Meals.FindAsync(MealId);
        if (meal != null)
        {
            _context.Meals.Remove(meal);
            await _context.SaveChangesAsync();
        }
    }


}
