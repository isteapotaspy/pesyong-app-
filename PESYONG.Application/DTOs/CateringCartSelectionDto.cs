using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.DTOs;

/// <summary>
/// Represents a selected meal item inside a catering package/cart.
/// </summary>
/// 
// CheckoutItemDto uses CateringCartSelectionDto
public class CateringCartSelectionDto
{
    /// <summary>
    /// Gets or sets the selected meal's ID.
    /// </summary>
    public int MealId { get; set; }

    /// <summary>
    /// Gets or sets the selected meal's name.
    /// </summary>
    public string MealName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected meal's price.
    /// </summary>
    public decimal MealPrice { get; set; }
}