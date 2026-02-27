namespace PESYONG.Domain.Entities;

public class CartItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Price { get; set; }
    public int Quantity { get; set; }
    public string Image { get; set; } = string.Empty;
    public string Type { get; set; } // package, short-order, kakanin
    public int? Pax { get; set; }
    public int ProductId { get; set; }
}

public class DeliveryInfo
{
    public string Address { get; set; } = string.Empty;
    public string Location { get; set; } = "poblacion"; // poblacion, outside
    public double? Distance { get; set; }
    public double DeliveryFee { get; set; }
}