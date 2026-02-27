using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PESYONG.Domain.Entities.Meals.MealProduct;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Enums;
using System;

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

    // Logic translated from Figma cart-page.txt
    public decimal CalculatedDeliveryFee =>
        LocationType == "poblacion" ? 15.00m : Math.Max(25.00m, 25.00m + (decimal)(Math.Floor(Distance - 1) * 10));

    public decimal Total => CurrentOrder.OrderTotalAmount + DeliveryFee;

    public decimal Subtotal => CurrentOrder.OrderTotalAmount;

    public decimal GrandTotal => Subtotal + DeliveryFee;

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

    public void InitializeNewOrder(int userId)
    {
        CurrentOrder = new Order
        {
            OrderID = Guid.NewGuid(), // Correct: Order uses Guid
            RecipientID = userId,     // Correct: User ID is int
            DeliveryStatus = DeliveryStatus.OnCart
        };
    }
}