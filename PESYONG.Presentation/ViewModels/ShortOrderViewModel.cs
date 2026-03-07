using Microsoft.UI.Xaml.Media.Imaging;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace PESYONG.Presentation.ViewModels;

/// <summary>
/// View model for short order items with UI-specific properties, mapping to updated Meal entity.
/// </summary>
public class ShortOrderViewModel : INotifyPropertyChanged
{
    private int _mealId;
    private string _mealName = string.Empty;
    private decimal _mealPrice;
    private int _selectedQuantity = 1;
    private int _cartQuantity;
    private decimal _totalPrice;
    private string _description = string.Empty;
    private int _stockQuantity;
    private int _minOrderQuantity = 1;
    private DeliveryType _deliveryType;
    private List<string> _mealTags = new();
    private byte[]? _imageBytes;

    public BitmapImage MealImage { get; } = new BitmapImage();

    /// <summary>
    /// Constructor to create ViewModel from Meal entity
    /// </summary>
    public ShortOrderViewModel(Meal meal, int cartQuantity = 0)
    {
        _mealId = meal.MealID ?? 0;
        _mealName = meal.MealName;
        _mealPrice = meal.MealPrice;
        _imageBytes = meal.ImageBytes;
        _description = meal.Description ?? string.Empty;
        _stockQuantity = meal.StockQuantity;
        _minOrderQuantity = meal.MinOrderQuantity;
        _deliveryType = meal.DeliveryType;
        _mealTags = meal.MealTags?.ToList() ?? new List<string>();
        _cartQuantity = cartQuantity;
        _selectedQuantity = _minOrderQuantity > 0 ? _minOrderQuantity : 1;
        _totalPrice = _mealPrice * _selectedQuantity;

        _ = LoadMealImageAsync();
    }

    /// <summary>
    /// Default constructor for XAML
    /// </summary>
    public ShortOrderViewModel() { }

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

    public decimal MealPrice
    {
        get => _mealPrice;
        set
        {
            if (_mealPrice != value)
            {
                _mealPrice = value;
                OnPropertyChanged();
                TotalPrice = _mealPrice * _selectedQuantity;
            }
        }
    }

    public byte[]? ImageBytes
    {
        get => _imageBytes;
        set
        {
            if (_imageBytes != value)
            {
                _imageBytes = value;
                OnPropertyChanged();
                _ = LoadMealImageAsync();
            }
        }
    }

    public int SelectedQuantity
    {
        get => _selectedQuantity;
        set
        {
            if (_selectedQuantity != value && value >= _minOrderQuantity && value <= _stockQuantity)
            {
                _selectedQuantity = value;
                OnPropertyChanged();
                TotalPrice = MealPrice * _selectedQuantity;
                OnPropertyChanged(nameof(CanIncreaseQuantity));
                OnPropertyChanged(nameof(CanDecreaseQuantity));
            }
        }
    }

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

    public int StockQuantity
    {
        get => _stockQuantity;
        set
        {
            if (_stockQuantity != value)
            {
                _stockQuantity = value;
                OnPropertyChanged();

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

    public int MinOrderQuantity
    {
        get => _minOrderQuantity;
        set
        {
            if (_minOrderQuantity != value)
            {
                _minOrderQuantity = value;
                OnPropertyChanged();

                if (_selectedQuantity < _minOrderQuantity)
                {
                    SelectedQuantity = _minOrderQuantity;
                }

                OnPropertyChanged(nameof(CanDecreaseQuantity));
            }
        }
    }

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

    public List<string> MealTags
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

    public bool IsAvailable => StockQuantity > 0;
    public bool HasItemsInCart => CartQuantity > 0;
    public string AvailabilityStatus => IsAvailable ? $"In Stock ({StockQuantity} available)" : "Out of Stock";
    public bool CanIncreaseQuantity => SelectedQuantity < StockQuantity;
    public bool CanDecreaseQuantity => SelectedQuantity > MinOrderQuantity;
    public string DeliveryTypeDisplay => DeliveryType.ToString();
    public string TagsDisplay => string.Join(", ", MealTags);

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void IncrementQuantity()
    {
        if (CanIncreaseQuantity)
        {
            SelectedQuantity++;
        }
    }

    public void DecrementQuantity()
    {
        if (CanDecreaseQuantity)
        {
            SelectedQuantity--;
        }
    }

    public void AddToCart()
    {
        CartQuantity += SelectedQuantity;
        SelectedQuantity = MinOrderQuantity;
    }

    public void RemoveFromCart(int quantity)
    {
        if (quantity <= CartQuantity)
        {
            CartQuantity -= quantity;
        }
    }

    public bool HasTag(string tag)
    {
        return MealTags.Contains(tag);
    }

    public string GetTagBadges()
    {
        return string.Join(" • ", MealTags);
    }

    private async Task LoadMealImageAsync()
    {
        if (_imageBytes == null || _imageBytes.Length == 0)
            return;

        try
        {
            using var stream = new MemoryStream(_imageBytes);
            using var randomAccessStream = stream.AsRandomAccessStream();
            await MealImage.SetSourceAsync(randomAccessStream);
            OnPropertyChanged(nameof(MealImage));
        }
        catch
        {
            // leave image empty if loading fails
        }
    }
}