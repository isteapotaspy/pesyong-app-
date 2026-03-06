using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PESYONG.Domain.Entities.Meals.MealProduct;

namespace PESYONG.Domain.Utilities;

public class MealProductItemsConverter : ValueConverter<ICollection<MealProductItem>, string>
{
    public MealProductItemsConverter()
        : base(
            v => ConvertToJson(v),
            v => ConvertFromJson(v),
            null)
    {
    }

    private static string ConvertToJson(ICollection<MealProductItem> items)
    {
        if (items == null || !items.Any())
            return "[]";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        return JsonSerializer.Serialize(items, options);
    }

    private static ICollection<MealProductItem> ConvertFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<MealProductItem>();

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Deserialize<List<MealProductItem>>(json, options)
                   ?? new List<MealProductItem>();
        }
        catch (JsonException)
        {
            // Handle deserialization errors gracefully
            return new List<MealProductItem>();
        }
    }
}
