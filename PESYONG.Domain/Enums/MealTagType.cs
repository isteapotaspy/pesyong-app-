using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.Domain.Enums
{
    public enum MealTagType
    {
        MealCategory = 1,    // Carbs, Viands, Desserts
        Dietary = 2,         // Vegan, GlutenFree, etc.
        Cuisine = 3,         // Filipino, Italian, etc.
        Ingredient = 4,      // Chicken, Seafood, etc.
        MealTime = 5,        // Breakfast, Lunch, Dinner
        Special = 6,       // Spicy, Kid-Friendly, etc.
        Kakanin = 7,
        Dessert = 8,
        MainDish = 9,
        SideDish = 10,  
        Beverage = 11,
        Viand = 12,
    }
}
