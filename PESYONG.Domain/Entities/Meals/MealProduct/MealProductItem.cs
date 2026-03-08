using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Entities.Meals.MealItem;

namespace PESYONG.Domain.Entities.Meals.MealProduct;

/// <summary>
/// This is a junction entity owned by MealProduct.a
/// Represents a product item included in a meal, specifying the 
/// associated meal, quantity, and any special requests.
/// </summary>

[Owned]
public class MealProductItem
{
    [ForeignKey(nameof(Meal))]
    public int MealID { get; set; }
    public virtual Meal? Meal { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; } = 1;

    [StringLength(100)]
    public string? RequestDescription { get; set; }

    [NotMapped]
    public decimal ItemPrice => Meal?.MealPrice * Quantity ?? 0m;

    public bool IsValid()
    {
        var validationContext = new ValidationContext(this);
        var validationResults = new List<ValidationResult>();

        return Validator.TryValidateObject(this, validationContext, validationResults, validateAllProperties: true);
    }
    public IEnumerable<string?> GetValidationErrors()
    {
        var validationContext = new ValidationContext(this);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(this, validationContext, validationResults, true);
        return validationResults.Select(vr => vr.ErrorMessage);
    }
}
