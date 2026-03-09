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
/// Represents a short order item in the presentation layer and exposes
/// UI-friendly properties, computed values, and image-loading behavior.
/// </summary>
public class ShortOrderViewModel : INotifyPropertyChanged
{
    private int _mealProductID;
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

    /// <summary>
    /// Gets the bitmap image used for displaying the meal photo in the UI.
    /// </summary>
    public BitmapImage MealImage { get; } = new BitmapImage();

    /// <summary>
    /// Initializes a new instance of the <see cref="ShortOrderViewModel"/> class
    /// using values from a meal entity.
    /// </summary>
    /// <param name="meal">The meal entity to map into the view model.</param>
    /// <param name="cartQuantity">The quantity of this meal already present in the cart.</param>
    public ShortOrderViewModel(Meal meal, int cartQuantity = 0)
    {
        _mealProductID = meal.MealID ?? 0;
        _mealName = meal.MealName;
        _mealPrice = meal.MealPrice;
        _imageBytes = meal.ImageBytes;
        _description = meal.Description ?? string.Empty;
        _stockQuantity = meal.StockQuantity;
        _minOrderQuantity = meal.MinOrderQuantity > 0 ? meal.MinOrderQuantity : 1;
        _deliveryType = meal.DeliveryType;
        _mealTags = meal.MealTags?.ToList() ?? new List<string>();
        _cartQuantity = cartQuantity;
        _selectedQuantity = _minOrderQuantity;
        _totalPrice = _mealPrice * _selectedQuantity;

        _ = LoadMealImageAsync();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShortOrderViewModel"/> class.
    /// </summary>
    public ShortOrderViewModel() { }

    /// <summary>
    /// Gets or sets the meal ID.
    /// </summary>
    public int MealProductID
    {
        get => _mealProductID;
        set
        {
            if (_mealProductID != value)
            {
                _mealProductID = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the meal name.
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
    /// Gets or sets the meal price.
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
                TotalPrice = _mealPrice * _selectedQuantity;
            }
        }
    }

    /// <summary>
    /// Gets or sets the raw image bytes for the meal.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the currently selected quantity for ordering.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the quantity of this item already in the cart.
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
    /// Gets or sets the total price based on the selected quantity.
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
    /// Gets or sets the meal description.
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
    /// Gets or sets the available stock quantity for the meal.
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

                if (_stockQuantity > 0 && _selectedQuantity > _stockQuantity)
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
    /// Gets or sets the minimum order quantity for the meal.
    /// </summary>
    public int MinOrderQuantity
    {
        get => _minOrderQuantity;
        set
        {
            int safeValue = value > 0 ? value : 1;

            if (_minOrderQuantity != safeValue)
            {
                _minOrderQuantity = safeValue;
                OnPropertyChanged();

                if (_selectedQuantity < _minOrderQuantity)
                {
                    SelectedQuantity = _minOrderQuantity;
                }

                OnPropertyChanged(nameof(CanDecreaseQuantity));
            }
        }
    }

    /// <summary>
    /// Gets or sets the supported delivery type for the meal.
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
    /// Gets or sets the collection of tags associated with the meal.
    /// </summary>
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

    /// <summary>
    /// Gets a value indicating whether the meal is currently available.
    /// </summary>
    public bool IsAvailable => StockQuantity > 0;

    /// <summary>
    /// Gets a value indicating whether this meal already has items in the cart.
    /// </summary>
    public bool HasItemsInCart => CartQuantity > 0;

    /// <summary>
    /// Gets the formatted stock availability text for display.
    /// </summary>
    public string AvailabilityStatus => IsAvailable ? $"In Stock ({StockQuantity} available)" : "Out of Stock";

    /// <summary>
    /// Gets a value indicating whether the selected quantity can still be increased.
    /// </summary>
    public bool CanIncreaseQuantity => SelectedQuantity < StockQuantity;

    /// <summary>
    /// Gets a value indicating whether the selected quantity can still be decreased.
    /// </summary>
    public bool CanDecreaseQuantity => SelectedQuantity > MinOrderQuantity;

    /// <summary>
    /// Gets the delivery type as a display string.
    /// </summary>
    public string DeliveryTypeDisplay => DeliveryType.ToString();

    /// <summary>
    /// Gets the formatted tag list as a comma-separated display string.
    /// </summary>
    public string TagsDisplay => string.Join(", ", MealTags);

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the changed property.</param>
    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Increases the selected quantity when allowed.
    /// </summary>
    public void IncrementQuantity()
    {
        if (CanIncreaseQuantity)
        {
            SelectedQuantity++;
        }
    }

    /// <summary>
    /// Decreases the selected quantity when allowed.
    /// </summary>
    public void DecrementQuantity()
    {
        if (CanDecreaseQuantity)
        {
            SelectedQuantity--;
        }
    }

    /// <summary>
    /// Loads the meal image from the stored image bytes into the bitmap source.
    /// </summary>
    /// <returns>A task representing the asynchronous image-loading operation.</returns>
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