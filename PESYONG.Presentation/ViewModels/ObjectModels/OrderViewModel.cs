using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Enums;
using PESYONG.Presentation.ViewModels.ObjectModels;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

public class OrderViewModel : INotifyPropertyChanged, IDataErrorInfo
{
    private Guid _orderID;
    private int? _receiptID;
    private int? _recipientID;
    private DateTime _orderDate = DateTime.Now;
    private DateTime? _estimatedDeliveryDate;
    private DateTime? _actualDeliveryDate;
    private DeliveryStatus _deliveryType = DeliveryStatus.OnCart;
    private DeliveryStatus _deliveryStatus = DeliveryStatus.Pending;
    private string? _address;
    private string? _trackingNumber;
    private string? _customerNotes;
    private string? _specialInstructions;
    private string _statusMessage = string.Empty;
    private string _validationSummary = string.Empty;

    private ObservableCollection<OrderMealProductViewModel> _orderItems = new();

    public event PropertyChangedEventHandler? PropertyChanged;

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
        set => SetProperty(ref _receiptID, value);
    }

    public int? RecipientID
    {
        get => _recipientID;
        set => SetProperty(ref _recipientID, value);
    }

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

    public DeliveryStatus DeliveryType
    {
        get => _deliveryType;
        set => SetProperty(ref _deliveryType, value);
    }

    public DeliveryStatus DeliveryStatus
    {
        get => _deliveryStatus;
        set => SetProperty(ref _deliveryStatus, value);
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
            if (SetProperty(ref _orderItems, value))
            {
                OnPropertyChanged(nameof(FormattedOrderTotalAmount));
            }
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

    public string RecipientIDText
    {
        get => RecipientID?.ToString() ?? string.Empty;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                RecipientID = null;
            }
            else if (int.TryParse(value.Trim(), out var parsed))
            {
                RecipientID = parsed;
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

    public string OrderDateDisplay => OrderDate.ToString("g");

    public decimal OrderTotalAmount => OrderItems.Sum(x => x.SubTotal);

    public string FormattedOrderTotalAmount => OrderTotalAmount.ToString("C");

    public string Error => string.Empty;

    public string this[string columnName]
    {
        get
        {
            try
            {
                if (columnName == nameof(OrderIDText))
                    return string.Empty;

                if (columnName == nameof(ReceiptIDText) || columnName == nameof(RecipientIDText))
                    return string.Empty;

                var context = new ValidationContext(this) { MemberName = columnName };
                var results = new List<ValidationResult>();

                var property = GetType().GetProperty(columnName);
                if (property == null)
                    return string.Empty;

                var value = property.GetValue(this);
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

        OrderItems = new ObservableCollection<OrderMealProductViewModel>(
            entity.OrderItems?.Select(OrderMealProductViewModel.CreateFromEntity) ??
            Enumerable.Empty<OrderMealProductViewModel>());

        RefreshTotals();
    }

    public Order ToEntity()
    {
        return new Order
        {
            OrderID = OrderID == Guid.Empty ? Guid.NewGuid() : OrderID,
            ReceiptID = ReceiptID,
            RecipientID = RecipientID,
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
        RecipientID = null;
        OrderDate = DateTime.Now;
        EstimatedDeliveryDate = null;
        ActualDeliveryDate = null;
        DeliveryType = DeliveryStatus.OnCart;
        DeliveryStatus = DeliveryStatus.Pending;
        Address = string.Empty;
        TrackingNumber = null;
        CustomerNotes = null;
        SpecialInstructions = null;
        OrderItems.Clear();
        StatusMessage = string.Empty;
        ValidationSummary = string.Empty;
        RefreshTotals();
    }

    public void RefreshTotals()
    {
        OnPropertyChanged(nameof(OrderTotalAmount));
        OnPropertyChanged(nameof(FormattedOrderTotalAmount));
    }

    public bool ValidateAll()
    {
        try
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(this);

            var isValid = Validator.TryValidateObject(this, context, results, true);

            foreach (var item in OrderItems)
            {
                if (!item.ValidateAll())
                {
                    isValid = false;
                }
            }

            ValidationSummary = string.Join(Environment.NewLine,
                results.Select(x => x.ErrorMessage).Where(x => !string.IsNullOrWhiteSpace(x)));

            if (!ValidateBusinessRules())
            {
                isValid = false;
            }

            OnPropertyChanged(string.Empty);
            return isValid;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Order validation failed: {ex}");
            ValidationSummary = "Validation failed unexpectedly.";
            return false;
        }
    }

    private bool ValidateBusinessRules()
    {
        if (OrderItems.Count == 0)
        {
            StatusMessage = "At least one order item is required.";
            return false;
        }

        if (ActualDeliveryDate.HasValue && ActualDeliveryDate < OrderDate)
        {
            StatusMessage = "Actual delivery date cannot be earlier than order date.";
            return false;
        }

        StatusMessage = string.Empty;
        return true;
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
}