using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Enums;
using PESYONG.Presentation.ViewModels.ObjectModels;

namespace PESYONG.Presentation.ViewModels;

/// <summary>
/// A reactive representation of an <see cref="Order"/> for the UI.
/// Fixes the "Object reference required" error by correctly referencing the instance field.
/// </summary>
public partial class OrderViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNewOrder))]
    private Guid _orderID;

    [ObservableProperty]
    private int? _receiptID;

    [ObservableProperty]
    private Guid? _customerID;

    [ObservableProperty]
    private AppUserViewModel? _recipient;

    [ObservableProperty]
    private AcknowledgementReceiptViewModel? _receipt;

    [ObservableProperty]
    private ObservableCollection<OrderMealProductViewModel> _orderItems = new();

    [ObservableProperty]
    [StringLength(100)]
    private string _productName;

    [ObservableProperty]
    [StringLength(100)]
    private string _productDescription;

    [ObservableProperty]
    [Required(ErrorMessage = "Order date is required")]
    [NotifyPropertyChangedFor(nameof(OrderDateDisplay))]
    private DateTime _orderDate = DateTime.Now;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EstimatedDeliveryDateDisplay))]
    private DateTime? _estimatedDeliveryDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActualDeliveryDateDisplay))]
    private DateTime? _actualDeliveryDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DeliveryTypeDisplay))]
    private DeliveryStatus _deliveryType = DeliveryStatus.OnCart;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DeliveryStatusDisplay))]
    [NotifyPropertyChangedFor(nameof(CanBeDelivered))]
    [NotifyPropertyChangedFor(nameof(IsCompleted))]
    private DeliveryStatus _deliveryStatus = DeliveryStatus.Pending;

    [ObservableProperty]
    [Required(ErrorMessage = "Address is required")]
    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private string? _address;

    [ObservableProperty]
    [StringLength(50, ErrorMessage = "Tracking number cannot exceed 50 characters")]
    [RegularExpression(@"^[A-Z0-9\-]+$", ErrorMessage = "Tracking number can only contain uppercase letters, numbers, and hyphens")]
    [NotifyDataErrorInfo]
    private string? _trackingNumber;

    [ObservableProperty]
    [StringLength(500, ErrorMessage = "Customer notes cannot exceed 500 characters")]
    [NotifyDataErrorInfo]
    private string? _customerNotes;

    [ObservableProperty]
    [StringLength(300, ErrorMessage = "Special instructions cannot exceed 300 characters")]
    [NotifyDataErrorInfo]
    private string? _specialInstructions;

    public OrderViewModel(Order entity)
    {
        if (entity != null)
        {
            OrderID = entity.OrderID;
            ReceiptID = entity.ReceiptID;
            CustomerID = entity.CustomerID;
            OrderDate = entity.OrderDate;
            EstimatedDeliveryDate = entity.EstimatedDeliveryDate;
            ActualDeliveryDate = entity.ActualDeliveryDate;
            DeliveryType = entity.DeliveryType;
            DeliveryStatus = entity.DeliveryStatus;
            Address = entity.Address;
            TrackingNumber = entity.TrackingNumber;
            CustomerNotes = entity.CustomerNotes;
            SpecialInstructions = entity.SpecialInstructions;

            // Convert OrderItems
            if (entity.OrderItems != null && entity.OrderItems.Any())
            {
                OrderItems = new ObservableCollection<OrderMealProductViewModel>(
                    entity.OrderItems.Select(OrderMealProductViewModel.FromEntity)
                );
            }
        }

        // Initialize property change handling
        PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != nameof(HasErrors) &&
                args.PropertyName != nameof(CanSave) &&
                args.PropertyName != nameof(OrderTotalAmount) &&
                args.PropertyName != nameof(ItemCount))
            {
                OnPropertyChanged(nameof(CanSave));
                OnPropertyChanged(nameof(OrderTotalAmount));
                OnPropertyChanged(nameof(ItemCount));
            }
        };

        ValidateAllProperties();
    }

    public OrderViewModel()
    {
        // Validate when properties change
        PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != nameof(HasErrors) &&
                args.PropertyName != nameof(CanSave) &&
                args.PropertyName != nameof(OrderTotalAmount) &&
                args.PropertyName != nameof(ItemCount))
            {
                OnPropertyChanged(nameof(CanSave));
                OnPropertyChanged(nameof(OrderTotalAmount));
                OnPropertyChanged(nameof(ItemCount));
            }
        };

        ValidateAllProperties();
    }

    // Computed properties
    public decimal OrderTotalAmount => OrderItems?.Sum(item => item.SubTotal) ?? 0;

    public int ItemCount => OrderItems?.Sum(item => item.MealProductOrderQty) ?? 0;

    public bool IsNewOrder => OrderID == Guid.Empty;

    public bool CanSave => !HasErrors && !string.IsNullOrWhiteSpace(Address) && OrderItems.Any();

    public bool CanBeDelivered => DeliveryStatus != DeliveryStatus.Delivered && DeliveryStatus != DeliveryStatus.Cancelled;

    public bool IsCompleted => DeliveryStatus == DeliveryStatus.Delivered;

    public string OrderDateDisplay => OrderDate.ToString("MMM dd, yyyy hh:mm tt");

    public string EstimatedDeliveryDateDisplay => EstimatedDeliveryDate?.ToString("MMM dd, yyyy") ?? "Not set";

    public string ActualDeliveryDateDisplay => ActualDeliveryDate?.ToString("MMM dd, yyyy") ?? "Not delivered";

    public string DeliveryTypeDisplay => DeliveryType.ToString();

    public string DeliveryStatusDisplay => DeliveryStatus.ToString();

    // Entity Mappers
    public static OrderViewModel FromEntity(Order entity)
    {
        if (entity == null)
            return new OrderViewModel();

        var viewModel = new OrderViewModel
        {
            OrderID = entity.OrderID,
            ReceiptID = entity.ReceiptID,
            CustomerID = entity.CustomerID,
            OrderDate = entity.OrderDate,
            EstimatedDeliveryDate = entity.EstimatedDeliveryDate,
            ActualDeliveryDate = entity.ActualDeliveryDate,
            DeliveryType = entity.DeliveryType,
            DeliveryStatus = entity.DeliveryStatus,
            Address = entity.Address,
            TrackingNumber = entity.TrackingNumber,
            CustomerNotes = entity.CustomerNotes,
            SpecialInstructions = entity.SpecialInstructions
        };

        // Convert OrderItems
        if (entity.OrderItems != null && entity.OrderItems.Any())
        {
            viewModel.OrderItems = new ObservableCollection<OrderMealProductViewModel>(
                entity.OrderItems.Select(OrderMealProductViewModel.FromEntity));
        }

        return viewModel;
    }

    public Order ToEntity()
    {
        var entity = new Order
        {
            OrderID = OrderID != Guid.Empty ? OrderID : Guid.NewGuid(),
            ReceiptID = ReceiptID,
            CustomerID = CustomerID,
            OrderDate = OrderDate,
            EstimatedDeliveryDate = EstimatedDeliveryDate,
            ActualDeliveryDate = ActualDeliveryDate,
            DeliveryType = DeliveryType,
            DeliveryStatus = DeliveryStatus,
            Address = Address?.Trim(),
            TrackingNumber = TrackingNumber?.Trim(),
            CustomerNotes = CustomerNotes?.Trim(),
            SpecialInstructions = SpecialInstructions?.Trim()
        };

        return entity;
    }

    // Order item management
    public void AddOrderItem(OrderMealProductViewModel item)
    {
        if (item != null)
        {
            // Check if item already exists
            var existingItem = OrderItems.FirstOrDefault(i => i.MealProductID == item.MealProductID);
            if (existingItem != null)
            {
                // Update quantity if item exists
                existingItem.MealProductOrderQty += item.MealProductOrderQty;
            }
            else
            {
                OrderItems.Add(item);
            }

            OnPropertyChanged(nameof(OrderTotalAmount));
            OnPropertyChanged(nameof(ItemCount));
        }
    }

    public void RemoveOrderItem(int mealProductId)
    {
        var item = OrderItems.FirstOrDefault(i => i.MealProductID == mealProductId);
        if (item != null)
        {
            OrderItems.Remove(item);
            OnPropertyChanged(nameof(OrderTotalAmount));
            OnPropertyChanged(nameof(ItemCount));
        }
    }

    public void UpdateOrderItemQuantity(int mealProductId, int newQuantity)
    {
        var item = OrderItems.FirstOrDefault(i => i.MealProductID == mealProductId);
        if (item != null)
        {
            item.MealProductOrderQty = newQuantity;
            OnPropertyChanged(nameof(OrderTotalAmount));
            OnPropertyChanged(nameof(ItemCount));
        }
    }

    public void ClearOrderItems()
    {
        OrderItems.Clear();
        OnPropertyChanged(nameof(OrderTotalAmount));
        OnPropertyChanged(nameof(ItemCount));
    }

    // Status management
    public void MarkAsDelivered(DateTime deliveryDate)
    {
        DeliveryStatus = DeliveryStatus.Delivered;
        ActualDeliveryDate = deliveryDate;
    }

    public void MarkAsShipped(string trackingNumber)
    {
        DeliveryStatus = DeliveryStatus.InTransit;
        TrackingNumber = trackingNumber;
    }

    public void CancelOrder()
    {
        DeliveryStatus = DeliveryStatus.Cancelled;
    }

    // Validation helpers
    public void ValidateAllProperties()
    {
        ValidateAllProperties();
    }

    public string GetErrorMessages()
    {
        if (!HasErrors) return string.Empty;

        var errors = GetErrors()
            .Select(error => $"{error.MemberNames.FirstOrDefault()}: {error.ErrorMessage}");

        return string.Join(Environment.NewLine, errors);
    }

    // Reset method
    public void Reset()
    {
        OrderID = Guid.Empty;
        ReceiptID = null;
        CustomerID = null;
        OrderItems.Clear();
        OrderDate = DateTime.Now;
        EstimatedDeliveryDate = null;
        ActualDeliveryDate = null;
        DeliveryType = DeliveryStatus.OnCart;
        DeliveryStatus = DeliveryStatus.Pending;
        Address = null;
        TrackingNumber = null;
        CustomerNotes = null;
        SpecialInstructions = null;

        ClearErrors();
    }

    // Copy method for creating similar orders
    public OrderViewModel CreateCopy()
    {
        var copy = new OrderViewModel
        {
            CustomerID = CustomerID,
            Address = Address,
            CustomerNotes = CustomerNotes,
            SpecialInstructions = SpecialInstructions,
            DeliveryType = DeliveryType
        };

        // Copy order items
        foreach (var item in OrderItems)
        {
            copy.AddOrderItem(item.Clone());
        }

        return copy;
    }
}