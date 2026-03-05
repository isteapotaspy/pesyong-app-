using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using PESYONG.Domain.Entities.Logistics;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Enums;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

public class DeliveryViewModel : ObservableValidator
{
    private Delivery _delivery = new();
    public Delivery GetDelivery() => _delivery;

    private bool _hasChanges = false;

    public Guid _orderID = Guid.Empty;
    public Guid OrderID 
    { 
        get => _orderID;
        set
        {
            if (_orderID != value)
            {
                _orderID = value;
                OnPropertyChanged();
                _delivery.OrderID = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    public List<DeliveryUpdate> _deliveryUpdates = new();
    public List<DeliveryUpdate> DeliveryUpdates
    { 
        get => _deliveryUpdates;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
                _delivery.DeliveryUpdates = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private int _deliveryPersonnelID;
    public int DeliveryPersonnelID {
        get => _deliveryPersonnelID;
        set
        {
            if (_deliveryPersonnelID != value)
            {
                _deliveryPersonnelID = value;
                OnPropertyChanged();
                _delivery.DeliveryPersonnelID = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private DeliveryStatus _deliveryStatus;
    public DeliveryStatus DeliveryStatus {
        get => _deliveryStatus;
        set
        {
            if (_deliveryStatus != value)
            {
                _deliveryStatus = value;
                OnPropertyChanged();
                _delivery.Status = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    public DateTime CreatedDate => _delivery.CreatedDate;

    private string _deliveryAddress = string.Empty;
    public string DeliveryAddress
    {
        get => _deliveryAddress;
        set
        {
            if (_deliveryAddress != value)
            {
                _deliveryAddress = value;
                OnPropertyChanged();
                _delivery.DeliveryAddress = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private string _trackingNumber;
    public string TrackingNumber
    {
        get => _trackingNumber;
        set
        {
            if (_trackingNumber != value)
            {
                _trackingNumber = value;
                OnPropertyChanged();
                _delivery.TrackingNumber = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private Decimal _shippingCost;
    public Decimal ShippingCost
    {
        get => _shippingCost;
        set
        {
            if (_shippingCost != value)
            {
                _shippingCost = value;
                OnPropertyChanged();
                _delivery.ShippingCost = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private string? _shippingMethod = string.Empty;
    public string? ShippingMethod
    {
        get => _shippingMethod;
        set
        {
            if (_shippingMethod != value)
            {
                _shippingMethod = value;
                OnPropertyChanged();
                _delivery.ShippingMethod = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private string? _carrierName = string.Empty;
    public string? CarrierName
    {
        get => _carrierName;
        set
        {
            if (_carrierName != value)
            {
                _carrierName = value;
                OnPropertyChanged();
                _delivery.CarrierName = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private string? _specialInstructions = string.Empty;
    public string? SpecialInstructions
    {
        get => _specialInstructions;
        set
        {
            if (_specialInstructions != value)
            {
                _specialInstructions = value;
                OnPropertyChanged();
                _delivery.SpecialInstructions = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private string? _deliveryNotes = string.Empty;
    public string? DeliveryNotes
    {
        get => _deliveryNotes;
        set
        {
            if (_deliveryNotes != value)
            {
                _deliveryNotes = value;
                OnPropertyChanged();
                _delivery.DeliveryNotes = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private string? _proofOfDelivery = string.Empty;
    public string? ProofOfDelivery
    {
        get => string.IsNullOrEmpty(_proofOfDelivery)
            ? "no_image.png"
            : "proof_image.png";
        set
        {
            if (_proofOfDelivery != value)
            {
                _proofOfDelivery = value;
                OnPropertyChanged();
                //Fix this later
                //OnPropertyChanged(ProofOfDeliveryImage);
                _delivery.ProofOfDelivery = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private BitmapImage ProofOfDeliveryImage { get => new BitmapImage(new Uri(_proofOfDelivery)); }

    private string? _recievedBy = string.Empty;
    public string? RecievedBy
    {
        get => _recievedBy;
        set
        {
            if (_recievedBy != value)
            {
                _recievedBy = value;
                OnPropertyChanged();
                //Fix this later
                //OnPropertyChanged(ProofOfDeliveryImage);
                _delivery.ReceivedBy = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }
    private DateTime? _recievedAt;
    public DateTime? RecievedAt
    {
        get => _recievedAt;
        set
        {
            if (_recievedAt != value)
            {
                _recievedAt = value;
                OnPropertyChanged();
                //Fix this later
                //OnPropertyChanged(ProofOfDeliveryImage);
                _delivery.ReceivedAt = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private DateTime? _estimatedDeliveryDate;
    public DateTime? EstimatedDeliveryDate
    {
        get => _estimatedDeliveryDate;
        set
        {
            if (_estimatedDeliveryDate != value)
            {
                _estimatedDeliveryDate = value;
                OnPropertyChanged();
                //Fix this later
                //OnPropertyChanged(ProofOfDeliveryImage);
                _delivery.EstimatedDeliveryDate = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private DateTime? _actualDeliveryDate;
    public DateTime? ActualDeliveryDate
    {
        get => _actualDeliveryDate;
        set
        {
            if (_actualDeliveryDate != value)
            {
                _actualDeliveryDate = value;
                OnPropertyChanged();
                //Fix this later
                //OnPropertyChanged(ProofOfDeliveryImage);
                _delivery.ActualDeliveryDate = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }
}
