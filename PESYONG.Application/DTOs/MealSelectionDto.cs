using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// <summary>
// Make sure to add your summaries here.
// </summary>


// DTO for selecting items in the UI
namespace PESYONG.ApplicationLogic.DTOs;

public partial class MealSelectionDto : ObservableObject
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; } // For the Checkbox logic
}

// DTO for displaying a Package (MealProduct)
public record PackageDisplayDto(int Id, string Name, string Description, decimal Price);