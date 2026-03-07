
using System.IO;
using System;

namespace PESYONG.Domain.Entities;
public class CartItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Price { get; set; }
    public int Quantity { get; set; }
    public byte[]? ImageBytes { get; set; }
    public string Type { get; set; } = string.Empty;
    public int? Pax { get; set; }
    public int ProductId { get; set; }

    public List<CateringCartSelection>? CateringSelections { get; set; }

    public double TotalPrice => Price * Quantity;

}

public class DeliveryInfo
{
    public string Address { get; set; } = string.Empty;
    public string Location { get; set; } = "poblacion"; // poblacion, outside
    public double? Distance { get; set; }
    public double DeliveryFee { get; set; }
}