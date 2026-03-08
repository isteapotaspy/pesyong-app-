using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens.Experimental;
using PESYONG.ApplicationLogic.Repositories;
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
    private readonly OrderRepository _orderRepository;

    [ObservableProperty]
    private Guid orderID;

    [ObservableProperty]
    private int? receiptID;

    [ObservableProperty]
    private int? recipientID;

    [ObservableProperty]
    private ObservableCollection<OrderMealProductViewModel> orderItems = new();

    [ObservableProperty]
    private DateTime orderDate = DateTime.Now;

    [ObservableProperty]
    private DateTime? estimatedDeliveryDate;

    [ObservableProperty]
    private DateTime? actualDeliveryDate;

    [ObservableProperty]
    private DeliveryStatus deliveryType = DeliveryStatus.OnCart;

    [ObservableProperty]
    private DeliveryStatus deliveryStatus = DeliveryStatus.Pending;

    [ObservableProperty]
    private string? address;

    [ObservableProperty]
    private string? trackingNumber;

    [ObservableProperty]
    private string? customerNotes;

    [ObservableProperty]
    private string? specialInstructions;

    [ObservableProperty]
    private bool hasValidationErrors;

    [ObservableProperty]
    private ObservableCollection<string> validationErrors = new();

    // Commands
    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }
    public IAsyncRelayCommand DeliverCommand { get; }
    public IAsyncRelayCommand ShipCommand { get; }
    public IAsyncRelayCommand CancelCommand { get; }
    public IRelayCommand AddItemCommand { get; }
    public IRelayCommand RemoveItemCommand { get; }
    public IRelayCommand ClearItemsCommand { get; }
    public IRelayCommand<OrderMealProductViewModel> UpdateItemQuantityCommand { get; }

    public OrderViewModel()
    {
        _orderRepository = App.Instance.Services.GetRequiredService<OrderRepository>();

        // Initialize commands
        SaveCommand = new AsyncRelayCommand(SaveOrderAsync, CanSaveOrder);
        LoadCommand = new AsyncRelayCommand(LoadOrderAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteOrderAsync, CanDeleteOrder);
        DeliverCommand = new AsyncRelayCommand(DeliverOrderAsync, CanDeliverOrder);
        ShipCommand = new AsyncRelayCommand(ShipOrderAsync, CanShipOrder);
        CancelCommand = new AsyncRelayCommand(CancelOrderAsync, CanCancelOrder);
        AddItemCommand = new RelayCommand<OrderMealProductViewModel>(AddOrderItem, CanAddOrderItem);
        RemoveItemCommand = new RelayCommand<int>(RemoveOrderItem, CanRemoveOrderItem);
        ClearItemsCommand = new RelayCommand(ClearOrderItems, CanClearOrderItems);
        UpdateItemQuantityCommand = new RelayCommand<OrderMealProductViewModel>(UpdateOrderItemQuantity);

        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName != nameof(HasValidationErrors) &&
                e.PropertyName != nameof(ValidationErrors))
            {
                Validate();
                SaveCommand.NotifyCanExecuteChanged();
                DeleteCommand.NotifyCanExecuteChanged();
                DeliverCommand.NotifyCanExecuteChanged();
                ShipCommand.NotifyCanExecuteChanged();
                CancelCommand.NotifyCanExecuteChanged();
                ClearItemsCommand.NotifyCanExecuteChanged();
            }
        };

        OrderItems.CollectionChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(OrderTotalAmount));
            OnPropertyChanged(nameof(ItemCount));
            SaveCommand.NotifyCanExecuteChanged();
            ClearItemsCommand.NotifyCanExecuteChanged();
        };
    }

    public OrderViewModel(Order entity) : this()
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        LoadFromEntity(entity);
    }

    public static OrderViewModel CreateFromEntity(Order entity)
    {
        var vm = new OrderViewModel();
        vm.LoadFromEntity(entity);
        return vm;
    }

    public void LoadFromEntity(Order entity)
    {
        OrderID = entity.OrderID;
        ReceiptID = entity.ReceiptID;
        RecipientID = entity.RecipientID;
        OrderDate = entity.OrderDate;
        EstimatedDeliveryDate = entity.EstimatedDeliveryDate;
        ActualDeliveryDate = entity.ActualDeliveryDate;
        DeliveryType = entity.DeliveryType;
        DeliveryStatus = entity.DeliveryStatus;
        Address = entity.Address;
        TrackingNumber = entity.TrackingNumber;
        CustomerNotes = entity.CustomerNotes;
        SpecialInstructions = entity.SpecialInstructions;

        // Convert OrderItems collection
        OrderItems.Clear();
        if (entity.OrderItems != null)
        {
            foreach (var item in entity.OrderItems)
            {
                OrderItems.Add(new OrderMealProductViewModel(item));
            }
        }
    }

    public Order ToEntity()
    {
        return new Order
        {
            OrderID = OrderID != Guid.Empty ? OrderID : Guid.NewGuid(),
            ReceiptID = ReceiptID,
            RecipientID = RecipientID,
            OrderDate = OrderDate,
            EstimatedDeliveryDate = EstimatedDeliveryDate,
            ActualDeliveryDate = ActualDeliveryDate,
            DeliveryType = DeliveryType,
            DeliveryStatus = DeliveryStatus,
            Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
            TrackingNumber = string.IsNullOrWhiteSpace(TrackingNumber) ? null : TrackingNumber.Trim(),
            CustomerNotes = string.IsNullOrWhiteSpace(CustomerNotes) ? null : CustomerNotes.Trim(),
            SpecialInstructions = string.IsNullOrWhiteSpace(SpecialInstructions) ? null : SpecialInstructions.Trim(),
            OrderItems = OrderItems.Select(item => item.ToEntity()).ToList()
        };
    }

    private bool CanSaveOrder() => !HasValidationErrors && !string.IsNullOrWhiteSpace(Address) && OrderItems.Any();

    private bool CanDeleteOrder() => !IsNewOrder;

    private bool CanDeliverOrder() => CanBeDelivered && DeliveryStatus == DeliveryStatus.InTransit;

    private bool CanShipOrder() => CanBeDelivered && DeliveryStatus == DeliveryStatus.Confirmed;

    private bool CanCancelOrder() => CanBeDelivered;

    private bool CanAddOrderItem(OrderMealProductViewModel? item) => item != null;

    private bool CanRemoveOrderItem(int mealProductId) => OrderItems.Any(
        i => i.MealProductID == mealProductId);

    private bool CanClearOrderItems() => OrderItems.Any();

    public void ClearOrderViewModel()
    {
        OrderID = Guid.Empty;
        ReceiptID = null;
        RecipientID = null;
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
    }

    private async Task SaveOrderAsync()
    {
        if (!CanSaveOrder() || _orderRepository == null) return;

        try
        {
            if (!IsNewOrder)
            {
                await _orderRepository.UpdateOrderAsync(ToEntity());
            }
            else
            {
                await _orderRepository.CreateOrderAsync(ToEntity());
            }
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", $"An error occurred while saving order: {ex.Message}", "OK");
        }
    }

    private async Task LoadOrderAsync()
    {
        if (IsNewOrder || _orderRepository == null) return;

        try
        {
            var order = await _orderRepository.GetOrderByIdAsync(OrderID);
            if (order != null)
            {
                LoadFromEntity(order);
            }
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", $"Failed to load order: {ex.Message}", "OK");
        }
    }

    private async Task DeleteOrderAsync()
    {
        if (IsNewOrder || _orderRepository == null) return;

        try
        {
            await _orderRepository.DeleteOrderAsync(OrderID);
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", $"An error occurred while deleting order: {ex.Message}", "OK");
        }
    }

    private async Task DeliverOrderAsync()
    {
        if (!CanDeliverOrder() || _orderRepository == null) return;

        try
        {
            MarkAsDelivered(DateTime.Now);
            await _orderRepository.UpdateOrderAsync(ToEntity());
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", $"An error occurred while marking order as delivered: {ex.Message}", "OK");
        }
    }

    private async Task ShipOrderAsync()
    {
        if (!CanShipOrder() || _orderRepository == null) return;

        try
        {
            // This would typically be called with a tracking number from UI input
            MarkAsShipped(TrackingNumber ?? "SHIP-" + OrderID.ToString().Substring(0, 8).ToUpper());
            await _orderRepository.UpdateOrderAsync(ToEntity());
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", $"An error occurred while shipping order: {ex.Message}", "OK");
        }
    }

    private async Task CancelOrderAsync()
    {
        if (!CanCancelOrder() || _orderRepository == null) return;

        try
        {
            CancelOrder();
            await _orderRepository.UpdateOrderAsync(ToEntity());
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", $"An error occurred while canceling order: {ex.Message}", "OK");
        }
    }

    private void AddOrderItem(OrderMealProductViewModel? item)
    {
        if (item == null) return;

        var existingItem = OrderItems.FirstOrDefault(i => i.MealProductID == item.MealProductID);
        if (existingItem != null)
        {
            existingItem.MealProductOrderQty += item.MealProductOrderQty;
        }
        else
        {
            OrderItems.Add(item);
        }
    }

    private void RemoveOrderItem(int mealProductId)
    {
        var item = OrderItems.FirstOrDefault(i => i.MealProductID == mealProductId);
        if (item != null)
        {
            OrderItems.Remove(item);
        }
    }

    private void UpdateOrderItemQuantity(OrderMealProductViewModel? item)
    {
        if (item == null) return;

        var existingItem = OrderItems.FirstOrDefault(i => i.MealProductID == item.MealProductID);
        if (existingItem != null)
        {
            existingItem.MealProductOrderQty = item.MealProductOrderQty;
        }
    }

    private void ClearOrderItems()
    {
        OrderItems.Clear();
    }

    private void Validate()
    {
        var entity = ToEntity();
        var errors = entity.GetValidationErrors().ToList();

        ValidationErrors.Clear();
        foreach (var error in errors)
        {
            ValidationErrors.Add(error ?? "Validation error");
        }

        HasValidationErrors = errors.Any();
    }

    private void ShowEventOnDebugConsole(string a, string b, string c)
    {
        Debug.Write($"[{a}] {c} : {b}");
    }

    // Computed properties
    public decimal OrderTotalAmount => OrderItems.Sum(item => item.SubTotal);

    public int ItemCount => OrderItems.Sum(item => item.MealProductOrderQty);

    public bool IsNewOrder => OrderID == Guid.Empty;

    public bool CanBeDelivered => DeliveryStatus != DeliveryStatus.Delivered && DeliveryStatus != DeliveryStatus.Cancelled;

    public bool IsCompleted => DeliveryStatus == DeliveryStatus.Delivered;

    public string OrderDateDisplay => OrderDate.ToString("MMM dd, yyyy hh:mm tt");

    public string EstimatedDeliveryDateDisplay => EstimatedDeliveryDate?.ToString("MMM dd, yyyy") ?? "Not set";

    public string ActualDeliveryDateDisplay => ActualDeliveryDate?.ToString("MMM dd, yyyy") ?? "Not delivered";

    public string DeliveryTypeDisplay => DeliveryType.ToString();

    public string DeliveryStatusDisplay => DeliveryStatus.ToString();

    public string OrderSummary => $"Order #{OrderID.ToString().Substring(0, 8)} - {ItemCount} items - {OrderTotalAmount:C}";

    partial void OnOrderItemsChanged(ObservableCollection<OrderMealProductViewModel> value)
    {
        OnPropertyChanged(nameof(OrderTotalAmount));
        OnPropertyChanged(nameof(ItemCount));
    }

    partial void OnDeliveryStatusChanged(DeliveryStatus value)
    {
        OnPropertyChanged(nameof(DeliveryStatusDisplay));
        OnPropertyChanged(nameof(CanBeDelivered));
        OnPropertyChanged(nameof(IsCompleted));
        DeliverCommand.NotifyCanExecuteChanged();
        ShipCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
    }

    partial void OnDeliveryTypeChanged(DeliveryStatus value)
    {
        OnPropertyChanged(nameof(DeliveryTypeDisplay));
    }

    partial void OnOrderDateChanged(DateTime value)
    {
        OnPropertyChanged(nameof(OrderDateDisplay));
    }

    partial void OnEstimatedDeliveryDateChanged(DateTime? value)
    {
        OnPropertyChanged(nameof(EstimatedDeliveryDateDisplay));
    }

    partial void OnActualDeliveryDateChanged(DateTime? value)
    {
        OnPropertyChanged(nameof(ActualDeliveryDateDisplay));
    }

    // Status management methods
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

    // Copy method for creating similar orders
    public OrderViewModel CreateCopy()
    {
        var copy = new OrderViewModel
        {
            RecipientID = RecipientID,
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

