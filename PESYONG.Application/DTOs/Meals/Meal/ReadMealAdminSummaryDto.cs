using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PESYONG.Domain.Entities.Meals.MealItem;

namespace PESYONG.ApplicationLogic.DTOs.Meals.Meal;

/// <summary>
/// This is used when you're admin reading each meal in ListView.
/// BTW this is so fucking radioactive make sure to CACHE THE IMAGES in the frontend to prevent bugs.
/// </summary>

public class ReadMealAdminSummaryDto
{
    public int MealID { get; set; }
    public string MealName { get; set; }
    public MealTag MealTag { get; set; }
    public byte[] MealImage { get; set; }

    [NotMapped] 
    [JsonIgnore] 
    public string ImageBase64 => MealImage != null ?
    $"data:{ImageContentType};base64,{Convert.ToBase64String(MealImage)}" :
    null;
}
