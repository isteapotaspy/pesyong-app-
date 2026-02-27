using PESYONG.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.Domain.Entities.Meals.MealItem;

/// <summary>
/// Represents a tag that categorizes a meal, such as dietary preference, cuisine, or meal type.
/// This has its own table called MealTags created by OPERATOR
/// </summary>
/// 
/// <remarks>
/// Meal tags are used to organize and filter meals based on specific attributes or classifications. 
/// </remarks>
/// 
public class MealTag
{
    [Key]
    public int MealTagID { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public MealTagType MealTagType { get; set; }
}