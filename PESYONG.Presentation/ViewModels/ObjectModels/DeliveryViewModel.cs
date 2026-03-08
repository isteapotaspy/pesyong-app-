using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using PESYONG.Domain.Entities.Logistics;
using PESYONG.Domain.Enums;
using PESYONG.Presentation.ViewModels.ObjectModels;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

public class DeliveryViewModel : INotifyPropertyChanged, IDataErrorInfo
{
    private int? _deliveryID;
    private Guid _orderID;
    private int? _deliveryPersonnelID;
    private DeliveryStatus _status = DeliveryStatus.Pending;
    private DateTime _createdDate = DateTime.Now;
    private string _deliveryAddress = string.Empty;
    private string? _trackingNumber;
    private decimal _shippingCost;
    private string? _shippingMethod;
    private string? _carrierName;
    private string? _specialInstructions;
    private string? _deliveryNotes;
    private string? _proofOfDelivery;
    private string? _currentLocation;
    private DateTime? _lastLocationUpdate;
    private bool _signatureRequired = true;
    private string? _receivedBy;
    private DateTime? _receivedAt;
    private DateTime? _estimatedDeliveryDate;
    private DateTime? _actualDeliveryDate;
    private string _statusMessage = string.Empty;
    private bool _isReadOnlyComputedFields;
    private ObservableCollection<DeliveryUpdateViewModel> _deliveryUpdates = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public int? DeliveryID
    {
        get => _deliveryID;
        set
        {
            if (SetProperty(ref _deliveryID, value))
            {
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    [Required(ErrorMessage = "Order ID is required.")]
    public Guid OrderID
    {
        get => _orderID;
        set => SetProperty(ref _orderID, value);
    }

    public int? DeliveryPersonnelID
    {
        get => _deliveryPersonnelID;
        set => SetProperty(ref _deliveryPersonnelID, value);
    }

    [Required(ErrorMessage = "Status is required.")]
    public DeliveryStatus Status
    {
        get => _status;
        set
        {
            if (SetProperty(ref _status, value))
            {
                RefreshComputedState();
            }
        }
    }

    [Required(ErrorMessage = "Created date is required.")]
    public DateTime CreatedDate
    {
        get => _createdDate;
        set
        {
            if (SetProperty(ref _createdDate, value))
            {
                OnPropertyChanged(nameof(CreatedDateDisplay));
            }
        }
    }

    [Required(ErrorMessage = "Delivery address is required.")]
    [StringLength(500, ErrorMessage = "Delivery address must not exceed 500 characters.")]
    public string DeliveryAddress
    {
        get => _deliveryAddress;
        set => SetProperty(ref _deliveryAddress, value);
    }

    [StringLength(50, ErrorMessage = "Tracking number must not exceed 50 characters.")]
    public string? TrackingNumber
    {
        get => _trackingNumber;
        set => SetProperty(ref _trackingNumber, value);
    }

    [Range(typeof(decimal), "0", "999999.99", ErrorMessage = "Shipping cost must be between 0 and 999999.99.")]
    public decimal ShippingCost
    {
        get => _shippingCost;
        set => SetProperty(ref _shippingCost, value);
    }

    [StringLength(50, ErrorMessage = "Shipping method must not exceed 50 characters.")]
    public string? ShippingMethod
    {
        get => _shippingMethod;
        set => SetProperty(ref _shippingMethod, value);
    }

    [StringLength(100, ErrorMessage = "Carrier name must not exceed 100 characters.")]
    public string? CarrierName
    {
        get => _carrierName;
        set => SetProperty(ref _carrierName, value);
    }

    [StringLength(1000, ErrorMessage = "Special instructions must not exceed 1000 characters.")]
    public string? SpecialInstructions
    {
        get => _specialInstructions;
        set => SetProperty(ref _specialInstructions, value);
    }

    [StringLength(2000, ErrorMessage = "Delivery notes must not exceed 2000 characters.")]
    public string? DeliveryNotes
    {
        get => _deliveryNotes;
        set => SetProperty(ref _deliveryNotes, value);
    }

    [StringLength(500, ErrorMessage = "Proof of delivery must not exceed 500 characters.")]
    public string? ProofOfDelivery
    {
        get => _proofOfDelivery;
        set => SetProperty(ref _proofOfDelivery, value);
    }

    [StringLength(500, ErrorMessage = "Current location must not exceed 500 characters.")]
    public string? CurrentLocation
    {
        get => _currentLocation;
        set => SetProperty(ref _currentLocation, value);
    }

    public DateTime? LastLocationUpdate
    {
        get => _lastLocationUpdate;
        set
        {
            if (SetProperty(ref _lastLocationUpdate, value))
            {
                OnPropertyChanged(nameof(LastLocationUpdateUi));
            }
        }
    }

    public bool SignatureRequired
    {
        get => _signatureRequired;
        set => SetProperty(ref _signatureRequired, value);
    }

    [StringLength(100, ErrorMessage = "Received by must not exceed 100 characters.")]
    public string? ReceivedBy
    {
        get => _receivedBy;
        set => SetProperty(ref _receivedBy, value);
    }

    public DateTime? ReceivedAt
    {
        get => _receivedAt;
        set
        {
            if (SetProperty(ref _receivedAt, value))
            {
                OnPropertyChanged(nameof(ReceivedAtUi));
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

    public ObservableCollection<DeliveryUpdateViewModel> DeliveryUpdates
    {
        get => _deliveryUpdates;
        set => SetProperty(ref _deliveryUpdates, value);
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

    public bool IsReadOnlyComputedFields
    {
        get => _isReadOnlyComputedFields;
        set
        {
            if (SetProperty(ref _isReadOnlyComputedFields, value))
            {
                OnPropertyChanged(nameof(AreEditableFieldsEnabled));
            }
        }
    }

    public bool AreEditableFieldsEnabled => !IsReadOnlyComputedFields;

    public string DisplayName =>
        DeliveryID.HasValue
            ? $"Delivery #{DeliveryID.Value} - {Status}"
            : "New Delivery";

    public string CreatedDateDisplay => CreatedDate.ToString("g");

    // -----------------------------
    // UI Safe Wrapper Properties
    // -----------------------------
    public string OrderIDText
    {
        get => OrderID == Guid.Empty ? string.Empty : OrderID.ToString();
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                OrderID = Guid.Empty;
            }
            else if (Guid.TryParse(value.Trim(), out var parsedGuid))
            {
                OrderID = parsedGuid;
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(OrderID));
        }
    }

    public string DeliveryPersonnelIDText
    {
        get => DeliveryPersonnelID?.ToString() ?? string.Empty;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                DeliveryPersonnelID = null;
            }
            else if (int.TryParse(value.Trim(), out var parsedInt))
            {
                DeliveryPersonnelID = parsedInt;
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(DeliveryPersonnelID));
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

    public DateTimeOffset? LastLocationUpdateUi
    {
        get => LastLocationUpdate.HasValue ? new DateTimeOffset(LastLocationUpdate.Value) : null;
        set
        {
            LastLocationUpdate = value?.DateTime;
            OnPropertyChanged();
        }
    }

    public DateTimeOffset? ReceivedAtUi
    {
        get => ReceivedAt.HasValue ? new DateTimeOffset(ReceivedAt.Value) : null;
        set
        {
            ReceivedAt = value?.DateTime;
            OnPropertyChanged();
        }
    }

    // IDataErrorInfo
    public string Error => string.Empty;

    public string this[string columnName]
    {
        get
        {
            try
            {
                if (columnName == nameof(OrderIDText))
                {
                    return OrderID == Guid.Empty ? "Order ID is required." : string.Empty;
                }

                if (columnName == nameof(DeliveryPersonnelIDText))
                {
                    return string.Empty;
                }

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
                Debug.WriteLine($"Indexer validation error for {columnName}: {ex}");
                return string.Empty;
            }
        }
    }

    public static DeliveryViewModel CreateFromEntity(Delivery entity)
    {
        var vm = new DeliveryViewModel();
        vm.LoadFromEntity(entity);
        return vm;
    }

    public void LoadFromEntity(Delivery entity)
    {
        DeliveryID = entity.DeliveryID;
        OrderID = entity.OrderID;
        DeliveryPersonnelID = entity.DeliveryPersonnelID;
        Status = entity.Status;
        CreatedDate = entity.CreatedDate;
        DeliveryAddress = entity.DeliveryAddress;
        TrackingNumber = entity.TrackingNumber;
        ShippingCost = entity.ShippingCost;
        ShippingMethod = entity.ShippingMethod;
        CarrierName = entity.CarrierName;
        SpecialInstructions = entity.SpecialInstructions;
        DeliveryNotes = entity.DeliveryNotes;
        ProofOfDelivery = entity.ProofOfDelivery;
        CurrentLocation = entity.CurrentLocation;
        LastLocationUpdate = entity.LastLocationUpdate;
        SignatureRequired = entity.SignatureRequired;
        ReceivedBy = entity.ReceivedBy;
        ReceivedAt = entity.ReceivedAt;
        EstimatedDeliveryDate = entity.EstimatedDeliveryDate;
        ActualDeliveryDate = entity.ActualDeliveryDate;

        DeliveryUpdates = new ObservableCollection<DeliveryUpdateViewModel>(
            entity.DeliveryUpdates?
                .OrderByDescending(x => x.UpdateDate)
                .Select(DeliveryUpdateViewModel.CreateFromEntity)
            ?? Enumerable.Empty<DeliveryUpdateViewModel>());

        RefreshComputedState();

        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(OrderIDText));
        OnPropertyChanged(nameof(DeliveryPersonnelIDText));
        OnPropertyChanged(nameof(EstimatedDeliveryDateUi));
        OnPropertyChanged(nameof(ActualDeliveryDateUi));
        OnPropertyChanged(nameof(LastLocationUpdateUi));
        OnPropertyChanged(nameof(ReceivedAtUi));
        OnPropertyChanged(nameof(CreatedDateDisplay));
    }

    public Delivery ToEntity()
    {
        return new Delivery
        {
            DeliveryID = DeliveryID ?? 0,
            OrderID = OrderID,
            DeliveryPersonnelID = DeliveryPersonnelID,
            Status = Status,
            CreatedDate = CreatedDate,
            DeliveryAddress = DeliveryAddress?.Trim() ?? string.Empty,
            TrackingNumber = NormalizeNullable(TrackingNumber),
            ShippingCost = ShippingCost,
            ShippingMethod = NormalizeNullable(ShippingMethod),
            CarrierName = NormalizeNullable(CarrierName),
            SpecialInstructions = NormalizeNullable(SpecialInstructions),
            DeliveryNotes = NormalizeNullable(DeliveryNotes),
            ProofOfDelivery = NormalizeNullable(ProofOfDelivery),
            CurrentLocation = NormalizeNullable(CurrentLocation),
            LastLocationUpdate = LastLocationUpdate,
            SignatureRequired = SignatureRequired,
            ReceivedBy = NormalizeNullable(ReceivedBy),
            ReceivedAt = ReceivedAt,
            EstimatedDeliveryDate = EstimatedDeliveryDate,
            ActualDeliveryDate = ActualDeliveryDate,
            DeliveryUpdates = DeliveryUpdates.Select(x => x.ToEntity()).ToList()
        };
    }

    public void ClearDeliveryViewModel()
    {
        DeliveryID = null;
        OrderID = Guid.Empty;
        DeliveryPersonnelID = null;
        Status = DeliveryStatus.Pending;
        CreatedDate = DateTime.Now;
        DeliveryAddress = string.Empty;
        TrackingNumber = null;
        ShippingCost = 0m;
        ShippingMethod = null;
        CarrierName = null;
        SpecialInstructions = null;
        DeliveryNotes = null;
        ProofOfDelivery = null;
        CurrentLocation = null;
        LastLocationUpdate = null;
        SignatureRequired = true;
        ReceivedBy = null;
        ReceivedAt = null;
        EstimatedDeliveryDate = null;
        ActualDeliveryDate = null;
        DeliveryUpdates.Clear();
        StatusMessage = string.Empty;

        RefreshComputedState();

        OnPropertyChanged(nameof(OrderIDText));
        OnPropertyChanged(nameof(DeliveryPersonnelIDText));
        OnPropertyChanged(nameof(EstimatedDeliveryDateUi));
        OnPropertyChanged(nameof(ActualDeliveryDateUi));
        OnPropertyChanged(nameof(LastLocationUpdateUi));
        OnPropertyChanged(nameof(ReceivedAtUi));
        OnPropertyChanged(nameof(CreatedDateDisplay));
    }

    public bool ValidateAll()
    {
        try
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(this);
            var isValid = Validator.TryValidateObject(this, context, results, true);

            if (!isValid)
            {
                Debug.WriteLine("Delivery validation failed:");
                foreach (var result in results)
                {
                    Debug.WriteLine(result.ErrorMessage);
                }
            }

            OnPropertyChanged(string.Empty);

            var businessRulesValid = ValidateBusinessRules();
            return isValid && businessRulesValid;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Delivery validation exception: {ex}");
            return false;
        }
    }

    public void RefreshComputedState()
    {
        IsReadOnlyComputedFields = Status == DeliveryStatus.Delivered;
        OnPropertyChanged(nameof(DisplayName));
    }

    private bool ValidateBusinessRules()
    {
        if (OrderID == Guid.Empty)
        {
            StatusMessage = "Order ID is required.";
            return false;
        }

        if (ReceivedAt.HasValue && string.IsNullOrWhiteSpace(ReceivedBy))
        {
            StatusMessage = "Received by is required when received date is set.";
            return false;
        }

        if (ActualDeliveryDate.HasValue && ActualDeliveryDate.Value < CreatedDate)
        {
            StatusMessage = "Actual delivery date cannot be earlier than created date.";
            return false;
        }

        if (Status == DeliveryStatus.Delivered && !ActualDeliveryDate.HasValue)
        {
            StatusMessage = "Actual delivery date is required when status is Delivered.";
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