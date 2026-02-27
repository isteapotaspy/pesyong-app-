using Microsoft.AspNetCore.Identity;
using PESYONG.Domain.Entities.Financial.AcknowledgementReceipts;
using PESYONG.Domain.Entities.Meals.MealProduct;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.Domain.Entities.Users.Identity;

/// <summary>
/// Represents an application user with extended navigation properties for managing user-specific data relationships.
/// Extends IdentityUser with integer primary key and provides navigation collections for:
/// - User's meal productors (or rather, meal packs) that they've made themselves [UserMealProducts] 
/// - User's cart, organized by each order (UserOrders)
/// - User's receipt acknowledgements (UserReceipts)
/// TECH DEBT: Make sure to lazy load all of this using DTOs later on.
/// </summary>

public class AppUser : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public AuthorizationType AuthorizationType { get; set; }
    public virtual ICollection<MealProduct> UserMealProducts { get; set; } = [];
    public virtual ICollection<Order> UserOrders { get; set; } = [];
    public virtual ICollection<AcknowledgementReceipt> UserReceipts { get; set; } = [];
}