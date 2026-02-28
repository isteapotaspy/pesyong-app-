using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Enums;

namespace PESYONG.ApplicationLogic.DTOs.Meals.Meal;

/// <summary>
/// This is used when you're admin reading each meal in ListView.
/// BTW this is so fucking radioactive make sure to CACHE THE IMAGES in the frontend to prevent bugs.
/// </summary>

/// <remarks>
/// TASK: Implement the fucking image converter here PUHLEAZE omggggggg
/// </remarks>

public class ReadMealAdminDto
{
    public int MealID { get; set; }
    public int OperatorID { get; set; }
    public string MealName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MealPrice { get; set; }
    public int StockQuantity { get; set; }
    public int MinOrderQuantity { get; set; }
    public List<MealTag> MealTags { get; set; }
    public DeliveryType DeliveryType { get; set; }
    public byte[]? BitmapImage { get; set; }
}
