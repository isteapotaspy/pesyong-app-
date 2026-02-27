using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.Domain.Entities;
public class PackageViand
{
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
}

public class CateringPackage
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string>? Viands { get; set; }
    public bool IsSelectable => Viands == null;
    public double Price { get; set; }
    public int Pax { get; set; }
    public string Image { get; set; } = string.Empty;
    public string? Badge { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool HasFreeDessert { get; set; }
}
