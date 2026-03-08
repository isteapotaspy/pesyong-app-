using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore.Internal;
using PESYONG.ApplicationLogic.DTOs;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.Domain.Entities;
using PESYONG.Domain.Entities.Meals.MealProduct;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Entities.Users;
using PESYONG.Domain.Enums;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace PESYONG.Presentation.ViewModels;

public partial class CheckoutViewModel : ObservableObject
{
    private readonly OrderRepository _orderRepository;

    public CheckoutViewModel(OrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    [ObservableProperty]
    private int _selectedPaymentIndex = 0;

    [ObservableProperty]
    private int _selectedLocationIndex = 0;

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _shippingAddress = string.Empty;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private string _location = "poblacion";

    [ObservableProperty]
    private string _distanceText = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _deliveryDate;

    [ObservableProperty]
    private TimeSpan _deliveryTime = DateTime.Now.TimeOfDay;

    [ObservableProperty]
    private string _paymentMethod = "cod";

    [ObservableProperty]
    private double _deliveryFee = 15;

    [ObservableProperty]
    private ObservableCollection<CartItem> _cartItems = new();

    public double Subtotal => CartItems.Sum(i => i.Price * i.Quantity);

    public double Total => Subtotal + DeliveryFee;

    public string SubtotalDisplay => $"₱{Subtotal:F2}";
    public string DeliveryFeeDisplay => $"₱{DeliveryFee:F2}";
    public string TotalDisplay => $"₱{Total:F2}";

    public void Initialize(ObservableCollection<CartItem> cartItems)
    {
        if (CartItems != null)
            CartItems.CollectionChanged -= CartItems_CollectionChanged;

        CartItems = cartItems ?? new ObservableCollection<CartItem>();
        CartItems.CollectionChanged += CartItems_CollectionChanged;

        RefreshComputedProperties();
        RecalculateDeliveryFee();
    }

    private void CartItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshComputedProperties();
    }

    partial void OnSelectedPaymentIndexChanged(int value)
    {
        PaymentMethod = value switch
        {
            0 => "cod",
            1 => "gcash",
            2 => "reservation",
            _ => "cod"
        };
    }

    partial void OnSelectedLocationIndexChanged(int value)
    {
        Location = value == 0 ? "poblacion" : "outside";
        RecalculateDeliveryFee();
    }

    partial void OnLocationChanged(string value)
    {
        SelectedLocationIndex = value == "outside" ? 1 : 0;
        RecalculateDeliveryFee();
    }

    partial void OnDistanceTextChanged(string value)
    {
        RecalculateDeliveryFee();
    }

    partial void OnDeliveryFeeChanged(double value)
    {
        RefreshComputedProperties();
    }

    private void RecalculateDeliveryFee()
    {
        if (Location == "poblacion")
        {
            DeliveryFee = 15;
        }
        else if (double.TryParse(DistanceText, out double distance))
        {
            DeliveryFee = Math.Max(25, Math.Floor(distance) * 5);
        }
        else
        {
            DeliveryFee = 0;
        }
    }

    private void RefreshComputedProperties()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(SubtotalDisplay));
        OnPropertyChanged(nameof(DeliveryFeeDisplay));
        OnPropertyChanged(nameof(TotalDisplay));
    }

    public bool Validate(out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            errorMessage = "Please enter the customer's first name.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            errorMessage = "Please enter the customer's last name.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(PhoneNumber))
        {
            errorMessage = "Please enter the customer's phone number.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(ShippingAddress))
        {
            errorMessage = "Please enter a delivery address.";
            return false;
        }

        if (DeliveryDate == null)
        {
            errorMessage = "Please select a delivery date.";
            return false;
        }

        if (Location == "outside" && !double.TryParse(DistanceText, out _))
        {
            errorMessage = "Please enter a valid distance for outside Poblacion delivery.";
            return false;
        }

        if (CartItems.Count == 0)
        {
            errorMessage = "Your cart is empty.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    [RelayCommand]
    private async Task<Guid?> PlaceOrderAsync()
    {
        if (!Validate(out _))
            return null;

        DateTime? estimatedDeliveryDate = null;

        if (DeliveryDate != null)
        {
            estimatedDeliveryDate = DeliveryDate.Value.Date + DeliveryTime;
        }

        var request = new CheckoutRequestDto
        {
            FirstName = FirstName,
            LastName = LastName,
            PhoneNumber = PhoneNumber,
            Email = string.IsNullOrWhiteSpace(Email) ? null : Email,
            Address = ShippingAddress,
            Location = Location,
            Distance = double.TryParse(DistanceText, out double distance) ? distance : null,
            DeliveryFee = DeliveryFee,
            EstimatedDeliveryDate = estimatedDeliveryDate,
            Notes = Notes,
            PaymentMethod = PaymentMethod,
            Items = CartItems.Select(i => new CheckoutItemDto
            {
                ProductID = i.ProductId,
                Quantity = i.Quantity,
                ItemPrice = (decimal)i.Price,
                Type = i.Type,
                CateringSelections = i.CateringSelections?.Select(x => new CateringCartSelectionDto
                {
                    MealId = x.MealId,
                    MealName = x.MealName,
                    Price = x.Price
                }).ToList()
            }).ToList()
        };

        var orderId = await _orderRepository.PlaceOrderAsync(request);
        return orderId;
    }

    public Task<Guid?> SubmitOrderAsync()
    {
        return PlaceOrderAsync();
    }

    public void ClearForm()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        PhoneNumber = string.Empty;
        Email = string.Empty;
        ShippingAddress = string.Empty;
        Notes = string.Empty;
        DistanceText = string.Empty;

        SelectedLocationIndex = 0;
        SelectedPaymentIndex = 0;

        DeliveryDate = null;
        DeliveryTime = DateTime.Now.TimeOfDay;

        RecalculateDeliveryFee();
        RefreshComputedProperties();
    }
}