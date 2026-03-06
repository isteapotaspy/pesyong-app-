using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Infrastructure;

namespace PESYONG.ApplicationLogic.Repositories
{
    public class MealRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public MealRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Creates a meal in the database.
        /// </summary>
        public async Task CreateMealAsync(Meal meal)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            context.Meals.Add(meal);
            Debug.WriteLine($"\n\nThe meal has ID of {meal.MealID} and is named {meal.MealName}\n\n");
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Creates a meal in the database and returns the created meal with its assigned MealID.
        /// </summary>
        public async Task<Meal> CreateMealAsyncReturnSelf(Meal meal)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            context.Meals.Add(meal);
            await context.SaveChangesAsync();

            Debug.WriteLine($"\n\nThe meal has ID of {meal.MealID} and is named {meal.MealName}\n\n");
            return meal;
        }

        /// <summary>
        /// Grabs a meal by its ID. Returns null if not found.
        /// </summary>
        public async Task<Meal?> GetMealByIdAsync(int id)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                return await context.Meals
                    .AsNoTracking()
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
        public async Task<List<Meal>> GetAllMealsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var meals = await context.Meals
                .AsNoTracking()
                .ToListAsync();

            if (meals == null || !meals.Any())
            {
                Debug.WriteLine("No meals found in database");
                return new List<Meal>();
            }

            Debug.WriteLine($"Retrieved {meals.Count} meals from database");
            return meals;
        }

        /// <summary>
        /// Allows for complex query to occur.
        /// </summary>
        public async Task<List<Meal>> GetMealsAsync(Func<IQueryable<Meal>, IQueryable<Meal>> queryBuilder)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            IQueryable<Meal> query = context.Meals.AsQueryable();
            query = queryBuilder(query);

            return await query.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Updates a meal. Pass through the entire meal object and update by its delta.
        /// </summary>
        public async Task UpdateMealAsync(Meal meal)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            context.Meals.Update(meal);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a meal in the database.
        /// </summary>
        public async Task DeleteMealAsync(int mealId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var meal = await context.Meals.FindAsync(mealId);
            if (meal != null)
            {
                context.Meals.Remove(meal);
                await context.SaveChangesAsync();
            }
        }
    }
}