using CommunityToolkit.Mvvm.ComponentModel;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Enums;
using System;
using System.Collections.Generic;

namespace PESYONG.Presentation.ViewModels;

/// <summary>
/// A reactive representation of an <see cref="Order"/> for the UI.
/// Fixes the "Object reference required" error by correctly referencing the instance field.
/// </summary>
public partial class OrderViewModel : ObservableObject
{
    [ObservableProperty]
    private Order _order;

    public OrderViewModel(Order order)
    {
        _order = order;
    }

    public Guid OrderID => _order.OrderID;
    public DateTime OrderDate => _order.OrderDate;

    /// <summary>
    /// Gets the collection of products included in this specific order.
    /// </summary>
    public ICollection<OrderMealProduct> OrderItems => _order.OrderItems;
    public decimal OrderTotalAmount => _order.OrderTotalAmount;

    public DeliveryStatus DeliveryStatus
    {
        get => _order.DeliveryStatus;
        set
        {
            if (_order.DeliveryStatus != value)
            {
                _order.DeliveryStatus = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets the timestamp when the order was fulfilled. 
    /// Returns null if the order is still in progress.
    /// </summary>
    public DateTime? ActualDeliveryDate
    {
        get => _order.ActualDeliveryDate;
        set
        {
            if (_order.ActualDeliveryDate != value)
            {
                _order.ActualDeliveryDate = value;
                OnPropertyChanged();
            }
        }
    }
}