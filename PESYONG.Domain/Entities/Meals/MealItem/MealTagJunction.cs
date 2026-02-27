using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.Domain.Entities.Meals.MealItem;

/// <summary>
/// Junction entity that uses many [MealTag]s to categorize a [Meal] in the domain model.
/// </summary>
public class MealTagJunction
{
    [ForeignKey(nameof(Meal))]
    public int? MealID { get; set; }

    [ForeignKey(nameof(MealTag))]
    public int? MealTagID { get; set; }

    // Navigation entities
    public virtual Meal? Meal { get; set; }
    public virtual MealTag? MealTag { get; set; }
}

