using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PESYONG.Domain.Entities.Logistics;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Enums;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

public partial class DeliveryViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNewDelivery))]
    private int _deliveryID;

    [ObservableProperty]
    private ObservableCollection<DeliveryUpdateViewModel> _deliveryUpdates = new();

    [ObservableProperty]
    [Required(ErrorMessage = "Order ID is required")]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private Guid _orderID;

    [ObservableProperty]
    private OrderViewModel? _order;

    [ObservableProperty]
    private int? _deliveryPersonnelID;

    [ObservableProperty]
    private AppUserViewModel? _deliveryPersonnel;

    [ObservableProperty]
    [Required(ErrorMessage = "Status is required")]
    [NotifyPropertyChangedFor(nameof(StatusDisplay))]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private DeliveryStatus _status = DeliveryStatus.Pending;

    [ObservableProperty]
    [Required(ErrorMessage = "Created date is required")]
    private DateTime _createdDate = DateTime.Now;

    [ObservableProperty]
    [Required(ErrorMessage = "Delivery address is required")]
    [StringLength(500, ErrorMessage = "Delivery address cannot exceed 500 characters")]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private string _deliveryAddress = string.Empty;

    [ObservableProperty]
    [StringLength(50, ErrorMessage = "Tracking number cannot exceed 50 characters")]
    [NotifyDataErrorInfo]
    private string? _trackingNumber;

    [ObservableProperty]
    [Required(ErrorMessage = "Shipping cost is required")]
    [Range(0, 999999.99, ErrorMessage = "Shipping cost must be between 0 and 999,999.99")]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private decimal _shippingCost;

    [ObservableProperty]
    [StringLength(50, ErrorMessage = "Shipping method cannot exceed 50 characters")]
    [NotifyDataErrorInfo]
    private string? _shippingMethod;

    [ObservableProperty]
    [StringLength(100, ErrorMessage = "Carrier name cannot exceed 100 characters")]
    [NotifyDataErrorInfo]
    private string? _carrierName;

    [ObservableProperty]
    [StringLength(1000, ErrorMessage = "Special instructions cannot exceed 1000 characters")]
    [NotifyDataErrorInfo]
    private string? _specialInstructions;

    [ObservableProperty]
    [StringLength(2000, ErrorMessage = "Delivery notes cannot exceed 2000 characters")]
    [NotifyDataErrorInfo]
    private string? _deliveryNotes;

    [ObservableProperty]
    [StringLength(500, ErrorMessage = "Proof of delivery cannot exceed 500 characters")]
    [NotifyDataErrorInfo]
    private string? _proofOfDelivery;

    [ObservableProperty]
    [StringLength(500, ErrorMessage = "Current location cannot exceed 500 characters")]
    [NotifyDataErrorInfo]
    private string? _currentLocation;

    [ObservableProperty]
    private DateTime? _lastLocationUpdate;

    [ObservableProperty]
    [Required(ErrorMessage = "Signature required flag is required")]
    private bool _signatureRequired = true;

    [ObservableProperty]
    [StringLength(100, ErrorMessage = "Received by cannot exceed 100 characters")]
    [NotifyDataErrorInfo]
    private string? _receivedBy;

    [ObservableProperty]
    private DateTime? _receivedAt;

    [ObservableProperty]
    private DateTime? _estimatedDeliveryDate;

    [ObservableProperty]
    private DateTime? _actualDeliveryDate;

    public DeliveryViewModel()
    {
        // Validate when properties change
        PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != nameof(HasErrors) && args.PropertyName != nameof(CanSave))
            {
                OnPropertyChanged(nameof(CanSave));
            }
        };

        ValidateAllProperties();
    }

    // Computed properties
    public string StatusDisplay => Status.ToString();

    public bool IsNewDelivery => DeliveryID == 0;

    public bool CanSave => !HasErrors && !string.IsNullOrWhiteSpace(DeliveryAddress);

    public bool IsDelivered => Status == DeliveryStatus.Delivered;

    public bool IsInTransit => Status == DeliveryStatus.InTransit;

    public string EstimatedDeliveryDisplay => EstimatedDeliveryDate?.ToString("MMM dd, yyyy hh:mm tt") ?? "Not set";

    public string ActualDeliveryDisplay => ActualDeliveryDate?.ToString("MMM dd, yyyy hh:mm tt") ?? "Not delivered";

    // Entity Mappers
    public static DeliveryViewModel FromEntity(Delivery entity)
    {
        if (entity == null)
            return new DeliveryViewModel();

        var viewModel = new DeliveryViewModel
        {
            DeliveryID = entity.DeliveryID,
            OrderID = entity.OrderID,
            DeliveryPersonnelID = entity.DeliveryPersonnelID,
            Status = entity.Status,
            CreatedDate = entity.CreatedDate,
            DeliveryAddress = entity.DeliveryAddress,
            TrackingNumber = entity.TrackingNumber,
            ShippingCost = entity.ShippingCost,
            ShippingMethod = entity.ShippingMethod,
            CarrierName = entity.CarrierName,
            SpecialInstructions = entity.SpecialInstructions,
            DeliveryNotes = entity.DeliveryNotes,
            ProofOfDelivery = entity.ProofOfDelivery,
            CurrentLocation = entity.CurrentLocation,
            LastLocationUpdate = entity.LastLocationUpdate,
            SignatureRequired = entity.SignatureRequired,
            ReceivedBy = entity.ReceivedBy,
            ReceivedAt = entity.ReceivedAt,
            EstimatedDeliveryDate = entity.EstimatedDeliveryDate,
            ActualDeliveryDate = entity.ActualDeliveryDate
        };

        // Convert DeliveryUpdates
        if (entity.DeliveryUpdates != null && entity.DeliveryUpdates.Any())
        {
            viewModel.DeliveryUpdates = new ObservableCollection<DeliveryUpdateViewModel>(
                entity.DeliveryUpdates.Select(DeliveryUpdateViewModel.FromEntity));
        }

        return viewModel;
    }

    public Delivery ToEntity()
    {
        var entity = new Delivery
        {
            DeliveryID = DeliveryID,
            OrderID = OrderID,
            DeliveryPersonnelID = DeliveryPersonnelID,
            Status = Status,
            CreatedDate = CreatedDate,
            DeliveryAddress = DeliveryAddress?.Trim() ?? string.Empty,
            TrackingNumber = TrackingNumber?.Trim(),
            ShippingCost = ShippingCost,
            ShippingMethod = ShippingMethod?.Trim(),
            CarrierName = CarrierName?.Trim(),
            SpecialInstructions = SpecialInstructions?.Trim(),
            DeliveryNotes = DeliveryNotes?.Trim(),
            ProofOfDelivery = ProofOfDelivery?.Trim(),
            CurrentLocation = CurrentLocation?.Trim(),
            LastLocationUpdate = LastLocationUpdate,
            SignatureRequired = SignatureRequired,
            ReceivedBy = ReceivedBy?.Trim(),
            ReceivedAt = ReceivedAt,
            EstimatedDeliveryDate = EstimatedDeliveryDate,
            ActualDeliveryDate = ActualDeliveryDate
        };

        return entity;
    }

    // Delivery update management
    public void AddDeliveryUpdate(DeliveryUpdateViewModel update)
    {
        if (update != null)
        {
            DeliveryUpdates.Add(update);
        }
    }

    public void RemoveDeliveryUpdate(int deliveryUpdateID)
    {
        var update = DeliveryUpdates.FirstOrDefault(u => u.DeliveryUpdateID == deliveryUpdateID);
        if (update != null)
        {
            DeliveryUpdates.Remove(update);
        }
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

    // Status management
    public void MarkAsDelivered(string receivedBy, DateTime deliveredAt, string? proofOfDelivery = null)
    {
        Status = DeliveryStatus.Delivered;
        ReceivedBy = receivedBy;
        ReceivedAt = deliveredAt;
        ActualDeliveryDate = deliveredAt;
        ProofOfDelivery = proofOfDelivery;
    }

    public void UpdateLocation(string location, string? notes = null)
    {
        CurrentLocation = location;
        LastLocationUpdate = DateTime.Now;

        if (!string.IsNullOrWhiteSpace(notes))
        {
            DeliveryNotes += (string.IsNullOrWhiteSpace(DeliveryNotes) ? "" : "\n") + $"{DateTime.Now:MMM dd, yyyy HH:mm}: {notes}";
        }
    }

    // Reset method
    public void Reset()
    {
        DeliveryID = 0;
        DeliveryUpdates.Clear();
        OrderID = Guid.Empty;
        DeliveryPersonnelID = null;
        Status = DeliveryStatus.Pending;
        CreatedDate = DateTime.Now;
        DeliveryAddress = string.Empty;
        TrackingNumber = null;
        ShippingCost = 0;
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

        ClearErrors();
    }
}