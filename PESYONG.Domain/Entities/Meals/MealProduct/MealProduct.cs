using PESYONG.Domain.Entities.Financial.Promos;
using PESYONG.Domain.Entities.Users.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PESYONG.Domain.Entities.Meals.MealProduct;

public class MealProduct
{
    [Key]
    public int MealProductID { get; set; }

    [ForeignKey(nameof(Owner))]
    public int? OwnerID { get; set; }

    [ForeignKey(nameof(Promo))]
    public int? PromoID { get; set; }

    public AppUser? Owner { get; set; }

    [Required]
    public bool IsCateringPackage { get; set; }

    public ICollection<MealProductItem> MealProductItems { get; set; } = [];

    public virtual Promo? Promo { get; set; }

    [Required]
    [StringLength(100)]
    public string ProductName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ProductDescription { get; set; }

    [NotMapped]
    public decimal ProductBasePrice => MealProductItems.Sum(item => item.ItemPrice);

    [NotMapped]
    public decimal DiscountAmount => ProductBasePrice - FinalPrice;

    [NotMapped]
    public decimal FinalPrice => Promo?.ApplyDiscount(ProductBasePrice) ?? ProductBasePrice;

    public bool IsValid()
    {
        var validationContext = new ValidationContext(this);
        var validationResults = new List<ValidationResult>();

        return Validator.TryValidateObject(this, validationContext, validationResults, validateAllProperties: true);
    }

    public IEnumerable<string> GetValidationErrors()
    {
        var validationContext = new ValidationContext(this);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(this, validationContext, validationResults, true);
        return validationResults.Select(vr => vr.ErrorMessage ?? string.Empty);
    }
}