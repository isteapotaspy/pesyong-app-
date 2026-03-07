using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PESYONG.Domain.Entities.Meals.MealProduct;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Enums;
using System;

namespace PESYONG.Presentation.ViewModels;

/// <summary>
/// Manages the shopping cart state and checkout logic.
/// Handles delivery fee calculations based on location, 
/// item quantity adjustments, and order initialization.
/// </summary>
public partial class CartViewModel : ObservableObject
{
    // Uses your Domain Model from audit.txt
    [ObservableProperty]
    private Order _currentOrder = new() { OrderID = Guid.NewGuid() };

    [ObservableProperty]
    private string _locationType = "poblacion"; // Match Figma: 'poblacion' | 'outside'

    [ObservableProperty]
    private double _distance = 1.0;

    [ObservableProperty] private decimal _deliveryFee;

    /// <summary>
    /// Calculates fee based on distance: Flat 15.00 for 'poblacion', 
    /// or base 25.00 + 10.00 per extra km for 'outside' locations.
    /// </summary>
    public decimal CalculatedDeliveryFee =>
        LocationType == "poblacion" ? 15.00m : Math.Max(25.00m, 25.00m + (decimal)(Math.Floor(Distance - 1) * 10));

    public decimal Total => CurrentOrder.OrderTotalAmount + DeliveryFee;

    public decimal Subtotal => CurrentOrder.OrderTotalAmount;

    public decimal GrandTotal => Subtotal + DeliveryFee;


    /// <summary>
    /// Maps a <see cref="MealProduct"/> to an <see cref="OrderMealProduct"/> 
    /// and attaches it to the current active order.
    /// </summary>
    [RelayCommand]
    private void AddProductToCart(MealProduct product)
    {
        // Fix: Assigning int to int, Guid to Guid
        var item = new OrderMealProduct
        {
            OrderID = CurrentOrder.OrderID,      // Guid
            MealProductID = product.MealProductID, // int
            ItemPrice = product.FinalPrice,       // decimal
            MealProductOrderQty = 1
        };
        CurrentOrder.OrderItems.Add(item);
        NotifyTotals();
    }

    private void NotifyTotals()
    {
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(CurrentOrder));
    }

    [RelayCommand]
    private void IncrementQuantity(OrderMealProduct item)
    {
        item.MealProductOrderQty++;
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(GrandTotal));
    }

    [RelayCommand]
    private void PlaceOrder()
    {
        // Set the address in domain model
        CurrentOrder.DeliveryStatus = DeliveryStatus.Pending;
        // Logic to save to DB via Service...
    }

    public void InitializeNewOrder(Guid userId)
    {
        CurrentOrder = new Order
        {
            OrderID = Guid.NewGuid(), // Correct: Order uses Guid
            CustomerID = userId,     // Correct: User ID is int
            DeliveryStatus = DeliveryStatus.OnCart
        };
    }
}