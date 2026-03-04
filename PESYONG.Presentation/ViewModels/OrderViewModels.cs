using PESYONG.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PESYONG.ApplicationLogic.ViewModels;

public class OrderViewModel
{
    public Guid OrderID { get; set; }
    public int? ReceiptID { get; set; }
    public int? RecipientID { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public DeliveryStatus DeliveryType { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; }
    public string Address { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string CustomerNotes { get; set; } = string.Empty;
    public string SpecialInstructions { get; set; } = string.Empty;
    public decimal OrderTotalAmount { get; set; }
    public bool HasReceipt => ReceiptID.HasValue;
    public string StatusDisplay => DeliveryStatus.ToString();
    public List<OrderMealProductViewModel> OrderItems { get; set; } = new();
}

public class CreateOrderViewModel
{
    public int RecipientID { get; set; }
    public string Address { get; set; } = string.Empty;
    public DeliveryStatus DeliveryType { get; set; } = DeliveryStatus.OnCart;
    public string CustomerNotes { get; set; } = string.Empty;
    public string SpecialInstructions { get; set; } = string.Empty;
    public List<OrderItemViewModel> Items { get; set; } = new();
}

public class OrderItemViewModel
{
    public int MealProductID { get; set; }
    public int MealProductOrderQty { get; set; }
    public decimal ItemPrice { get; set; }
}

public class OrderMealProductViewModel
{
    public Guid OrderID { get; set; }
    public int MealProductID { get; set; }
    public string MealProductName { get; set; } = string.Empty;
    public decimal ItemPrice { get; set; }
    public int MealProductOrderQty { get; set; }
    public decimal SubTotal => MealProductOrderQty * ItemPrice;
}

public class UpdateOrderStatusViewModel
{
    public Guid OrderID { get; set; }
    public DeliveryStatus NewDeliveryStatus { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public DateTime? EstimatedDeliveryDate { get; set; }
}

public class OrderSummaryViewModel
{
    public Guid OrderID { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; }
    public decimal OrderTotalAmount { get; set; }
    public int ItemCount { get; set; }
    public bool IsDelivered => DeliveryStatus == DeliveryStatus.Delivered;
}

public class CartViewModel
{
    public Guid OrderID { get; set; }
    public List<OrderMealProductViewModel> Items { get; set; } = new();

    // FIXED: Add using System.Linq and fix Sum method
    public decimal TotalAmount => Items.Sum(item => item.SubTotal);
    public int TotalItems => Items.Sum(item => item.MealProductOrderQty);
}
