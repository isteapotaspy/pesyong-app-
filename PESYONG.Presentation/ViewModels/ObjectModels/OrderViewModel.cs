using Microsoft.UI.Xaml.Controls;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System.Text.RegularExpressions;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

public class OrderViewModel : INotifyPropertyChanged, IDataErrorInfo
{
    private Guid _orderID;
    private int? _receiptID;
    private Guid? _customerID;
    private ObservableCollection<OrderMealProductViewModel> _orderItems = new();
    private DateTime _orderDate = DateTime.Now;
    private DateTime? _estimatedDeliveryDate;
    private DateTime? _actualDeliveryDate;

    private DeliveryType _deliveryType = DeliveryType.Delivery;
    private DeliveryStatus _deliveryStatus = DeliveryStatus.Pending;

    private string? _address;
    private string? _trackingNumber;
    private string? _customerNotes;
    private string? _specialInstructions;
    private string _statusMessage = string.Empty;
    private string _validationSummary = string.Empty;

    private string _customerFirstName = string.Empty;
    private string _customerLastName = string.Empty;
    private string _customerPhoneNumber = string.Empty;
    private string _customerEmail = string.Empty;


    public event PropertyChangedEventHandler? PropertyChanged;

    public OrderViewModel()
    {
        HookOrderItems(_orderItems);
    }

    public OrderViewModel(Order entity) : this()
    {
        LoadFromEntity(entity);
    }

    public Guid OrderID
    {
        get => _orderID;
        set
        {
            if (SetProperty(ref _orderID, value))
            {
                OnPropertyChanged(nameof(DisplayName));
                OnPropertyChanged(nameof(OrderIDText));
            }
        }
    }

    public int? ReceiptID
    {
        get => _receiptID;
        set
        {
            if (SetProperty(ref _receiptID, value))
            {
                OnPropertyChanged(nameof(ReceiptIDText));
            }
        }
    }

    public Guid? CustomerID
    {
        get => _customerID;
        set => SetProperty(ref _customerID, value);
    }

    [Required(ErrorMessage = "Order date is required.")]
    public DateTime OrderDate
    {
        get => _orderDate;
        set
        {
            if (SetProperty(ref _orderDate, value))
            {
                OnPropertyChanged(nameof(OrderDateDisplay));
                OnPropertyChanged(nameof(OrderDateUi));
            }
        }
    }

    public DateTime? EstimatedDeliveryDate
    {
        get => _estimatedDeliveryDate;
        set
        {
            if (SetProperty(ref _estimatedDeliveryDate, value))
            {
                OnPropertyChanged(nameof(EstimatedDeliveryDateUi));
            }
        }
    }

    public DateTime? ActualDeliveryDate
    {
        get => _actualDeliveryDate;
        set
        {
            if (SetProperty(ref _actualDeliveryDate, value))
            {
                OnPropertyChanged(nameof(ActualDeliveryDateUi));
            }
        }
    }

    public DeliveryType DeliveryType
    {
        get => _deliveryType;
        set
        {
            if (SetProperty(ref _deliveryType, value))
            {
                OnPropertyChanged(nameof(DeliveryTypeDisplay));
            }
        }
    }

    public DeliveryStatus DeliveryStatus
    {
        get => _deliveryStatus;
        set
        {
            if (SetProperty(ref _deliveryStatus, value))
            {
                OnPropertyChanged(nameof(DeliveryStatusDisplay));
                OnPropertyChanged(nameof(DeliveryStatusBadgeBrush));
            }
        }
    }

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
    public string? Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    [StringLength(50, ErrorMessage = "Tracking number cannot exceed 50 characters.")]
    [RegularExpression(@"^[A-Z0-9\-]+$", ErrorMessage = "Tracking number can only contain uppercase letters, numbers, and hyphens.")]
    public string? TrackingNumber
    {
        get => _trackingNumber;
        set => SetProperty(ref _trackingNumber, value);
    }

    [StringLength(500, ErrorMessage = "Customer notes cannot exceed 500 characters.")]
    public string? CustomerNotes
    {
        get => _customerNotes;
        set => SetProperty(ref _customerNotes, value);
    }

    [StringLength(300, ErrorMessage = "Special instructions cannot exceed 300 characters.")]
    public string? SpecialInstructions
    {
        get => _specialInstructions;
        set => SetProperty(ref _specialInstructions, value);
    }

    public ObservableCollection<OrderMealProductViewModel> OrderItems
    {
        get => _orderItems;
        set
        {
            if (_orderItems == value)
                return;

            UnhookOrderItems(_orderItems);
            _orderItems = value ?? new ObservableCollection<OrderMealProductViewModel>();
            HookOrderItems(_orderItems);

            OnPropertyChanged();
            RefreshTotals();
            OnPropertyChanged(nameof(ItemCount));
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (SetProperty(ref _statusMessage, value))
            {
                OnPropertyChanged(nameof(HasStatusMessage));
            }
        }
    }

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    public string ValidationSummary
    {
        get => _validationSummary;
        set => SetProperty(ref _validationSummary, value);
    }

    public string DisplayName =>
    OrderID == Guid.Empty
        ? "New Order"
        : !string.IsNullOrWhiteSpace(CustomerFullName)
            ? $"{CustomerFullName} - {OrderID.ToString()[..8]}"
            : $"Order {OrderID.ToString()[..8]}";

    public string OrderIDText
    {
        get => OrderID == Guid.Empty ? string.Empty : OrderID.ToString();
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                OrderID = Guid.Empty;
            }
            else if (Guid.TryParse(value.Trim(), out var parsed))
            {
                OrderID = parsed;
            }

            OnPropertyChanged();
        }
    }

    public string ReceiptIDText
    {
        get => ReceiptID?.ToString() ?? string.Empty;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                ReceiptID = null;
            }
            else if (int.TryParse(value.Trim(), out var parsed))
            {
                ReceiptID = parsed;
            }

            OnPropertyChanged();
        }
    }

    public string CustomerIDText
    {
        get => CustomerID?.ToString() ?? string.Empty;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                CustomerID = null;
            }
            else if (Guid.TryParse(value.Trim(), out var parsed))
            {
                CustomerID = parsed;
            }

            OnPropertyChanged();
        }
    }

    public DateTimeOffset OrderDateUi
    {
        get => new DateTimeOffset(OrderDate);
        set
        {
            OrderDate = value.DateTime;
            OnPropertyChanged();
        }
    }

    public DateTimeOffset? EstimatedDeliveryDateUi
    {
        get => EstimatedDeliveryDate.HasValue ? new DateTimeOffset(EstimatedDeliveryDate.Value) : null;
        set
        {
            EstimatedDeliveryDate = value?.DateTime;
            OnPropertyChanged();
        }
    }

    public DateTimeOffset? ActualDeliveryDateUi
    {
        get => ActualDeliveryDate.HasValue ? new DateTimeOffset(ActualDeliveryDate.Value) : null;
        set
        {
            ActualDeliveryDate = value?.DateTime;
            OnPropertyChanged();
        }
    }

    public string CustomerFirstName
    {
        get => _customerFirstName;
        set
        {
            if (SetProperty(ref _customerFirstName, value))
            {
                OnPropertyChanged(nameof(CustomerFullName));
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    public string CustomerLastName
    {
        get => _customerLastName;
        set
        {
            if (SetProperty(ref _customerLastName, value))
            {
                OnPropertyChanged(nameof(CustomerFullName));
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    public string CustomerPhoneNumber
    {
        get => _customerPhoneNumber;
        set => SetProperty(ref _customerPhoneNumber, value);
    }

    public string CustomerEmail
    {
        get => _customerEmail;
        set => SetProperty(ref _customerEmail, value);
    }

    public string CustomerFullName =>
        string.Join(" ", new[] { CustomerFirstName, CustomerLastName }
            .Where(x => !string.IsNullOrWhiteSpace(x)));

    public string OrderDateDisplay => OrderDate.ToString("g");

    public decimal OrderTotalAmount => OrderItems.Sum(x => x.SubTotal);

    public string FormattedOrderTotalAmount => $"PHP {OrderTotalAmount:N2}";

    public int ItemCount => OrderItems.Sum(x => x.MealProductOrderQty);

    public bool HasErrors =>
        !string.IsNullOrWhiteSpace(this[nameof(OrderDate)]) ||
        !string.IsNullOrWhiteSpace(this[nameof(Address)]) ||
        !string.IsNullOrWhiteSpace(this[nameof(TrackingNumber)]) ||
        !string.IsNullOrWhiteSpace(this[nameof(CustomerNotes)]) ||
        !string.IsNullOrWhiteSpace(this[nameof(SpecialInstructions)]);

    public bool CanSave => !HasErrors && ValidateBusinessRules(false);

    public string Error => string.Empty;

    public string this[string columnName]
    {
        get
        {
            try
            {
                if (columnName == nameof(OrderIDText) ||
                    columnName == nameof(ReceiptIDText) ||
                    columnName == nameof(CustomerIDText))
                {
                    return string.Empty;
                }

                var property = GetType().GetProperty(columnName);
                if (property == null)
                    return string.Empty;

                var value = property.GetValue(this);
                var context = new ValidationContext(this) { MemberName = columnName };
                var results = new List<ValidationResult>();

                var valid = Validator.TryValidateProperty(value, context, results);
                return valid ? string.Empty : results.FirstOrDefault()?.ErrorMessage ?? string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OrderViewModel validation indexer failed: {ex}");
                return string.Empty;
            }
        }
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
        CustomerID = entity.CustomerID;
        OrderDate = entity.OrderDate;
        EstimatedDeliveryDate = entity.EstimatedDeliveryDate;
        ActualDeliveryDate = entity.ActualDeliveryDate;
        DeliveryType = (DeliveryType)entity.DeliveryType;
        DeliveryStatus = entity.DeliveryStatus;
        Address = entity.Address;
        TrackingNumber = entity.TrackingNumber;
        CustomerNotes = entity.CustomerNotes;
        SpecialInstructions = entity.SpecialInstructions;

        OrderItems = new ObservableCollection<OrderMealProductViewModel>(
            entity.OrderItems?.Select(OrderMealProductViewModel.CreateFromEntity)
            ?? Enumerable.Empty<OrderMealProductViewModel>());
        
        CustomerFirstName = entity.Customer?.FirstName ?? string.Empty;
        CustomerLastName = entity.Customer?.LastName ?? string.Empty;
        CustomerPhoneNumber = entity.Customer?.PhoneNumber ?? string.Empty;
        CustomerEmail = entity.Customer?.Email ?? string.Empty;

        RefreshTotals();
    }

    public Order ToEntity()
    {
        return new Order
        {
            OrderID = OrderID == Guid.Empty ? Guid.NewGuid() : OrderID,
            ReceiptID = ReceiptID,
            CustomerID = CustomerID,
            OrderDate = OrderDate,
            EstimatedDeliveryDate = EstimatedDeliveryDate,
            ActualDeliveryDate = ActualDeliveryDate,
            DeliveryType = DeliveryType,
            DeliveryStatus = DeliveryStatus,
            Address = NormalizeNullable(Address),
            TrackingNumber = NormalizeNullable(TrackingNumber),
            CustomerNotes = NormalizeNullable(CustomerNotes),
            SpecialInstructions = NormalizeNullable(SpecialInstructions),
            OrderItems = OrderItems.Select(x => x.ToEntity()).ToList()
        };
    }

    public void Clear()
    {
        OrderID = Guid.Empty;
        ReceiptID = null;
        CustomerID = null;
        OrderItems.Clear();
        OrderDate = DateTime.Now;
        EstimatedDeliveryDate = null;
        ActualDeliveryDate = null;
        DeliveryType = DeliveryType.Delivery;
        DeliveryStatus = DeliveryStatus.Pending;
        Address = string.Empty;
        TrackingNumber = null;
        CustomerNotes = null;
        SpecialInstructions = null;

        CustomerFirstName = string.Empty;
        CustomerLastName = string.Empty;
        CustomerPhoneNumber = string.Empty;
        CustomerEmail = string.Empty;

        StatusMessage = string.Empty;
        ValidationSummary = string.Empty;
        RefreshTotals();
        OnPropertyChanged(nameof(ItemCount));
    }

    public void RefreshTotals()
    {
        OnPropertyChanged(nameof(OrderTotalAmount));
        OnPropertyChanged(nameof(FormattedOrderTotalAmount));
        OnPropertyChanged(nameof(ItemCount));
        OnPropertyChanged(nameof(CanSave));
    }

    public bool ValidateAll()
    {
        try
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(this);
            var isValid = Validator.TryValidateObject(this, context, results, true);
            var businessValid = ValidateBusinessRules(true);

            ValidationSummary = string.Join(
                Environment.NewLine,
                results.Select(x => x.ErrorMessage)
                       .Where(x => !string.IsNullOrWhiteSpace(x)));

            if (!businessValid && !string.IsNullOrWhiteSpace(StatusMessage))
            {
                ValidationSummary = string.IsNullOrWhiteSpace(ValidationSummary)
                    ? StatusMessage
                    : ValidationSummary + Environment.NewLine + StatusMessage;
            }

            OnPropertyChanged(string.Empty);
            return isValid && businessValid;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Order validation failed: {ex}");
            ValidationSummary = "Validation failed unexpectedly.";
            return false;
        }
    }

    private bool ValidateBusinessRules(bool setMessage)
    {
        if (OrderItems.Count == 0)
        {
            if (setMessage)
                StatusMessage = "At least one order item is required.";
            return false;
        }

        if (ActualDeliveryDate.HasValue && ActualDeliveryDate.Value < OrderDate)
        {
            if (setMessage)
                StatusMessage = "Actual delivery date cannot be earlier than order date.";
            return false;
        }

        if (setMessage)
            StatusMessage = string.Empty;

        return true;
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private void HookOrderItems(ObservableCollection<OrderMealProductViewModel> items)
    {
        items.CollectionChanged += OrderItems_CollectionChanged;

        foreach (var item in items)
        {
            item.PropertyChanged += OrderItem_PropertyChanged;
        }
    }

    private void UnhookOrderItems(ObservableCollection<OrderMealProductViewModel> items)
    {
        items.CollectionChanged -= OrderItems_CollectionChanged;

        foreach (var item in items)
        {
            item.PropertyChanged -= OrderItem_PropertyChanged;
        }
    }

    private void OrderItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (OrderMealProductViewModel item in e.OldItems)
            {
                item.PropertyChanged -= OrderItem_PropertyChanged;
            }
        }

        if (e.NewItems != null)
        {
            foreach (OrderMealProductViewModel item in e.NewItems)
            {
                item.PropertyChanged += OrderItem_PropertyChanged;
            }
        }

        RefreshTotals();
    }

    private void OrderItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OrderMealProductViewModel.ItemPrice) ||
            e.PropertyName == nameof(OrderMealProductViewModel.MealProductOrderQty) ||
            e.PropertyName == nameof(OrderMealProductViewModel.SubTotal))
        {
            RefreshTotals();
        }
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string DeliveryStatusDisplay => FormatEnumName(DeliveryStatus.ToString());

    public string DeliveryTypeDisplay => FormatEnumName(DeliveryType.ToString());

    public SolidColorBrush DeliveryStatusBadgeBrush => DeliveryStatus switch
    {
        DeliveryStatus.Delivered => new SolidColorBrush(Colors.ForestGreen),
        DeliveryStatus.Cancelled => new SolidColorBrush(Colors.IndianRed),
        DeliveryStatus.Failed => new SolidColorBrush(Colors.IndianRed),
        DeliveryStatus.Returned => new SolidColorBrush(Colors.IndianRed),

        DeliveryStatus.OutForDelivery => new SolidColorBrush(Colors.DodgerBlue),
        DeliveryStatus.InTransit => new SolidColorBrush(Colors.DodgerBlue),
        DeliveryStatus.Arrived => new SolidColorBrush(Colors.DodgerBlue),

        DeliveryStatus.Preparing => new SolidColorBrush(Colors.DarkOrange),
        DeliveryStatus.ReadyForPickup => new SolidColorBrush(Colors.DarkOrange),

        DeliveryStatus.Scheduled => new SolidColorBrush(Colors.Goldenrod),
        DeliveryStatus.Confirmed => new SolidColorBrush(Colors.Goldenrod),

        DeliveryStatus.Pending => new SolidColorBrush(Colors.Gray),
        DeliveryStatus.Attempted => new SolidColorBrush(Colors.SaddleBrown),
        DeliveryStatus.OnCart => new SolidColorBrush(Colors.DimGray),

        _ => new SolidColorBrush(Colors.Gray)
    };

    private static string FormatEnumName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return Regex.Replace(value, "([a-z])([A-Z])", "$1 $2");
    }

}