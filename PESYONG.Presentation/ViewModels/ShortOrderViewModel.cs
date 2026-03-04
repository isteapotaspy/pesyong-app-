using Microsoft.UI.Xaml.Data;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PESYONG.Presentation.ViewModels;

/// <summary>
/// View model for short order items with UI-specific properties, mapping to updated Meal entity.
/// </summary>
public class ShortOrderViewModel : INotifyPropertyChanged
{
    private int _mealId;
    private string _mealName;
    private decimal _mealPrice;
    private string _imageSourceString;
    private int _selectedQuantity = 1;
    private int _cartQuantity;
    private decimal _totalPrice;
    private string _description;
    private int _stockQuantity;
    private int _minOrderQuantity = 1;
    private DeliveryType _deliveryType;
    private List<MealTagType> _mealTags = new List<MealTagType>();

    /// <summary>
    /// Constructor to create ViewModel from Meal entity
    /// </summary>
    public ShortOrderViewModel(Meal meal, int cartQuantity = 0)
    {
        _mealId = meal.MealID;
        _mealName = meal.MealName;
        _mealPrice = meal.MealPrice;
        _imageSourceString = meal.ImageSourceString ?? string.Empty;
        _description = meal.Description ?? string.Empty;
        _stockQuantity = meal.StockQuantity;
        _minOrderQuantity = meal.MinOrderQuantity;
        _deliveryType = meal.DeliveryType;
        _mealTags = meal.MealTags?.ToList() ?? new List<MealTagType>();
        _cartQuantity = cartQuantity;
        _totalPrice = _mealPrice * _selectedQuantity;
    }

    /// <summary>
    /// Default constructor for XAML
    /// </summary>
    public ShortOrderViewModel() { }

    /// <summary>
    /// Meal ID from the domain model.
    /// </summary>
    public int MealID
    {
        get => _mealId;
        set
        {
            if (_mealId != value)
            {
                _mealId = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Name of the meal.
    /// </summary>
    public string MealName
    {
        get => _mealName;
        set
        {
            if (_mealName != value)
            {
                _mealName = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Price of the meal.
    /// </summary>
    public decimal MealPrice
    {
        get => _mealPrice;
        set
        {
            if (_mealPrice != value)
            {
                _mealPrice = value;
                OnPropertyChanged();
                // Recalculate total price when base price changes
                TotalPrice = _mealPrice * _selectedQuantity;
            }
        }
    }

    /// <summary>
    /// Image source string for the meal.
    /// </summary>
    public string ImageSourceString
    {
        get => _imageSourceString;
        set
        {
            if (_imageSourceString != value)
            {
                _imageSourceString = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Quantity selected by user for this item.
    /// </summary>
    public int SelectedQuantity
    {
        get => _selectedQuantity;
        set
        {
            if (_selectedQuantity != value && value >= 0 && value <= _stockQuantity)
            {
                _selectedQuantity = value;
                OnPropertyChanged();
                // Update total price when quantity changes
                TotalPrice = MealPrice * _selectedQuantity;
                OnPropertyChanged(nameof(CanIncreaseQuantity));
                OnPropertyChanged(nameof(CanDecreaseQuantity));
            }
        }
    }

    /// <summary>
    /// Quantity of this item currently in cart.
    /// </summary>
    public int CartQuantity
    {
        get => _cartQuantity;
        set
        {
            if (_cartQuantity != value)
            {
                _cartQuantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasItemsInCart));
            }
        }
    }

    /// <summary>
    /// Total price for selected quantity.
    /// </summary>
    public decimal TotalPrice
    {
        get => _totalPrice;
        set
        {
            if (_totalPrice != value)
            {
                _totalPrice = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Description of the short order item.
    /// </summary>
    public string Description
    {
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Available stock quantity from domain model.
    /// </summary>
    public int StockQuantity
    {
        get => _stockQuantity;
        set
        {
            if (_stockQuantity != value)
            {
                _stockQuantity = value;
                OnPropertyChanged();
                // Ensure SelectedQuantity doesn't exceed stock
                if (_selectedQuantity > _stockQuantity)
                {
                    SelectedQuantity = _stockQuantity;
                }
                OnPropertyChanged(nameof(IsAvailable));
                OnPropertyChanged(nameof(AvailabilityStatus));
                OnPropertyChanged(nameof(CanIncreaseQuantity));
            }
        }
    }

    /// <summary>
    /// Minimum order quantity from domain model.
    /// </summary>
    public int MinOrderQuantity
    {
        get => _minOrderQuantity;
        set
        {
            if (_minOrderQuantity != value)
            {
                _minOrderQuantity = value;
                OnPropertyChanged();
                // Ensure SelectedQuantity meets minimum requirement
                if (_selectedQuantity < _minOrderQuantity)
                {
                    SelectedQuantity = _minOrderQuantity;
                }
                OnPropertyChanged(nameof(CanDecreaseQuantity));
            }
        }
    }

    /// <summary>
    /// Delivery type for the meal.
    /// </summary>
    public DeliveryType DeliveryType
    {
        get => _deliveryType;
        set
        {
            if (_deliveryType != value)
            {
                _deliveryType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DeliveryTypeDisplay));
            }
        }
    }

    /// <summary>
    /// Meal tags for categorization.
    /// </summary>
    public List<MealTagType> MealTags
    {
        get => _mealTags;
        set
        {
            if (_mealTags != value)
            {
                _mealTags = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TagsDisplay));
            }
        }
    }

    /// <summary>
    /// Helper property to check if item is available (has stock)
    /// </summary>
    public bool IsAvailable => StockQuantity > 0;

    /// <summary>
    /// Helper property to check if item has items in cart
    /// </summary>
    public bool HasItemsInCart => CartQuantity > 0;

    /// <summary>
    /// Helper property to show availability status
    /// </summary>
    public string AvailabilityStatus => IsAvailable ? $"In Stock ({StockQuantity} available)" : "Out of Stock";

    /// <summary>
    /// Helper property for quantity validation
    /// </summary>
    public bool CanIncreaseQuantity => SelectedQuantity < StockQuantity;

    /// <summary>
    /// Helper property for quantity validation
    /// </summary>
    public bool CanDecreaseQuantity => SelectedQuantity > MinOrderQuantity;

    /// <summary>
    /// Display text for delivery type
    /// </summary>
    public string DeliveryTypeDisplay => DeliveryType.ToString();

    /// <summary>
    /// Display text for meal tags
    /// </summary>
    public string TagsDisplay => string.Join(", ", MealTags.Select(t => t.ToString()));

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Method to increment quantity with validation
    /// </summary>
    public void IncrementQuantity()
    {
        if (CanIncreaseQuantity)
        {
            SelectedQuantity++;
        }
    }

    /// <summary>
    /// Method to decrement quantity with validation
    /// </summary>
    public void DecrementQuantity()
    {
        if (CanDecreaseQuantity)
        {
            SelectedQuantity--;
        }
    }

    /// <summary>
    /// Method to update cart quantity and reset selected quantity
    /// </summary>
    public void AddToCart()
    {
        CartQuantity += SelectedQuantity;
        SelectedQuantity = MinOrderQuantity; // Reset to minimum after adding to cart
    }

    /// <summary>
    /// Method to remove items from cart
    /// </summary>
    public void RemoveFromCart(int quantity)
    {
        if (quantity <= CartQuantity)
        {
            CartQuantity -= quantity;
        }
    }

    /// <summary>
    /// Method to check if meal has specific tag
    /// </summary>
    public bool HasTag(MealTagType tag)
    {
        return MealTags.Contains(tag);
    }

    /// <summary>
    /// Method to get badge-style display for tags
    /// </summary>
    public string GetTagBadges()
    {
        return string.Join(" • ", MealTags.Select(t => t.ToString()));
    }
}
