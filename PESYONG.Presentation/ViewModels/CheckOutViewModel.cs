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

/// <summary>
/// Represents the checkout view model responsible for customer information,
/// cart totals, delivery details, validation, and order submission.
/// </summary>
public partial class CheckoutViewModel : ObservableObject
{
    private readonly OrderRepository _orderRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckoutViewModel"/> class.
    /// </summary>
    /// <param name="orderRepository">The repository used to place customer orders.</param>
    public CheckoutViewModel(OrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Gets or sets the selected payment option index.
    /// </summary>
    [ObservableProperty]
    private int _selectedPaymentIndex = 0;

    /// <summary>
    /// Gets or sets the selected location option index.
    /// </summary>
    [ObservableProperty]
    private int _selectedLocationIndex = 0;

    /// <summary>
    /// Gets or sets the customer's first name.
    /// </summary>
    [ObservableProperty]
    private string _firstName = string.Empty;

    /// <summary>
    /// Gets or sets the customer's last name.
    /// </summary>
    [ObservableProperty]
    private string _lastName = string.Empty;

    /// <summary>
    /// Gets or sets the customer's phone number.
    /// </summary>
    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    /// <summary>
    /// Gets or sets the customer's email address.
    /// </summary>
    [ObservableProperty]
    private string _email = string.Empty;

    /// <summary>
    /// Gets or sets the customer's shipping or delivery address.
    /// </summary>
    [ObservableProperty]
    private string _shippingAddress = string.Empty;

    /// <summary>
    /// Gets or sets additional notes for the order.
    /// </summary>
    [ObservableProperty]
    private string _notes = string.Empty;

    /// <summary>
    /// Gets or sets the selected location type.
    /// </summary>
    [ObservableProperty]
    private string _location = "poblacion";

    /// <summary>
    /// Gets or sets the distance text entered for delivery calculation.
    /// </summary>
    [ObservableProperty]
    private string _distanceText = string.Empty;

    /// <summary>
    /// Gets or sets the selected delivery date.
    /// </summary>
    [ObservableProperty]
    private DateTimeOffset? _deliveryDate;

    /// <summary>
    /// Gets or sets the selected delivery time.
    /// </summary>
    [ObservableProperty]
    private TimeSpan _deliveryTime = DateTime.Now.TimeOfDay;

    /// <summary>
    /// Gets or sets the selected payment method.
    /// </summary>
    [ObservableProperty]
    private string _paymentMethod = "cod";

    /// <summary>
    /// Gets or sets the computed delivery fee.
    /// </summary>
    [ObservableProperty]
    private double _deliveryFee = 15;

    /// <summary>
    /// Gets or sets the cart items included in the checkout.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<CartItem> _cartItems = new();

    /// <summary>
    /// Gets the subtotal amount for all cart items.
    /// </summary>
    public double Subtotal => CartItems.Sum(i => i.Price * i.Quantity);

    /// <summary>
    /// Gets the total amount including delivery fee.
    /// </summary>
    public double Total => Subtotal + DeliveryFee;

    /// <summary>
    /// Gets the formatted subtotal display string.
    /// </summary>
    public string SubtotalDisplay => $"₱{Subtotal:F2}";

    /// <summary>
    /// Gets the formatted delivery fee display string.
    /// </summary>
    public string DeliveryFeeDisplay => $"₱{DeliveryFee:F2}";

    /// <summary>
    /// Gets the formatted total display string.
    /// </summary>
    public string TotalDisplay => $"₱{Total:F2}";

    /// <summary>
    /// Initializes the view model with the provided cart items and prepares
    /// computed properties and delivery fee values.
    /// </summary>
    /// <param name="cartItems">The cart items to bind to the checkout view model.</param>
    public void Initialize(ObservableCollection<CartItem> cartItems)
    {
        if (CartItems != null)
            CartItems.CollectionChanged -= CartItems_CollectionChanged;

        CartItems = cartItems ?? new ObservableCollection<CartItem>();
        CartItems.CollectionChanged += CartItems_CollectionChanged;

        RefreshComputedProperties();
        RecalculateDeliveryFee();
    }

    /// <summary>
    /// Handles cart collection changes and refreshes computed totals.
    /// </summary>
    /// <param name="sender">The source of the collection change.</param>
    /// <param name="e">The collection change event data.</param>
    private void CartItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshComputedProperties();
    }

    /// <summary>
    /// Updates the payment method when the selected payment index changes.
    /// </summary>
    /// <param name="value">The selected payment index.</param>
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

    /// <summary>
    /// Updates the location type and recalculates delivery fee when the selected location index changes.
    /// </summary>
    /// <param name="value">The selected location index.</param>
    partial void OnSelectedLocationIndexChanged(int value)
    {
        Location = value == 0 ? "poblacion" : "outside";
        RecalculateDeliveryFee();
    }

    /// <summary>
    /// Updates the selected location index and recalculates delivery fee when the location value changes.
    /// </summary>
    /// <param name="value">The updated location value.</param>
    partial void OnLocationChanged(string value)
    {
        SelectedLocationIndex = value == "outside" ? 1 : 0;
        RecalculateDeliveryFee();
    }

    /// <summary>
    /// Recalculates the delivery fee when the distance text changes.
    /// </summary>
    /// <param name="value">The updated distance text value.</param>
    partial void OnDistanceTextChanged(string value)
    {
        RecalculateDeliveryFee();
    }

    /// <summary>
    /// Refreshes computed total-related properties when the delivery fee changes.
    /// </summary>
    /// <param name="value">The updated delivery fee value.</param>
    partial void OnDeliveryFeeChanged(double value)
    {
        RefreshComputedProperties();
    }

    /// <summary>
    /// Recalculates the delivery fee based on the selected location and entered distance.
    /// </summary>
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

    /// <summary>
    /// Raises property change notifications for computed total and display properties.
    /// </summary>
    private void RefreshComputedProperties()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(SubtotalDisplay));
        OnPropertyChanged(nameof(DeliveryFeeDisplay));
        OnPropertyChanged(nameof(TotalDisplay));
    }

    /// <summary>
    /// Validates the checkout form and returns an error message when validation fails.
    /// </summary>
    /// <param name="errorMessage">The validation error message, if any.</param>
    /// <returns><c>true</c> if the form is valid; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Validates the checkout form, builds the checkout request,
    /// and submits the order to the repository.
    /// </summary>
    /// <returns>
    /// The created order ID if the order was placed successfully; otherwise, <c>null</c>.
    /// </returns>
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
                    MealPrice = x.Price
                }).ToList()
            }).ToList()
        };

        var orderId = await _orderRepository.PlaceOrderAsync(request);
        return orderId;
    }

    /// <summary>
    /// Submits the current checkout form as an order.
    /// </summary>
    /// <returns>
    /// The created order ID if the order was placed successfully; otherwise, <c>null</c>.
    /// </returns>
    public Task<Guid?> SubmitOrderAsync()
    {
        return PlaceOrderAsync();
    }

    /// <summary>
    /// Clears all customer input fields and resets checkout selections to their default values.
    /// </summary>
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