using PESYONG.Domain.Entities.Meals.MealProduct;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PESYONG.Domain.Entities.Orders;

/// <summary>
/// This junction entity stores the foreign keys linking the item to its parent [Order]
/// </summary>
/// 

public class OrderMealProduct
{

    [ForeignKey(nameof(OrderID))]
    public Guid OrderID { get; set; }
    public Order? Order { get; set; }

    [ForeignKey(nameof(MealProductID))]
    public int MealProductID { get; set; }
    public virtual MealProduct? MealProduct { get; set; }

    // Fix: Rename this to Price at time of order and make it a decimal
    [Column(TypeName = "decimal(18,2)")]
    public decimal ItemPrice
    {
        get; set;
    }

    // Order item attributes
    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
    public int MealProductOrderQty { get; set; }
    [NotMapped]
    public decimal SubTotal => MealProductOrderQty * ItemPrice;

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