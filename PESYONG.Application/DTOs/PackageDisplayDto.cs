using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.Application.DTOs;

/// <remarks>
/// Add this to another folder in /DTOs named "Display".
/// </remarks>

public class PackageDisplayDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string PriceDisplay => Price.ToString("C");
}
