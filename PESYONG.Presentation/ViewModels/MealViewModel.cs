using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.Domain.Entities.Financial;
using PESYONG.Domain.Entities.Financial.Promos;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Meals.MealProduct;
using PESYONG.Domain.Entities.Users.Identity;
using PESYONG.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.Presentation.ViewModels.Admin;

/// <summary>
/// This handles the UI - Server data transfer for Meals.
/// This handles UI actions, coordinates model updates and
/// server communication, as well as transforms data for display.
/// </summary>
/// 

public partial class MealViewModel : ObservableValidator
{
    // reference to the model
    private Meal _meal = new();
    public Meal GetMeal() => _meal;

    // reference to the repository (or API if you have an actual backend)
    private readonly MealRepository? _mealRepository;

    // page specific shits
    private bool _hasChanges = false;
    public bool HasChanges
    {
        get => _hasChanges;
        set => SetProperty(ref _hasChanges, value);
    }

    public int? MealID { get => _meal.MealID; }
    public int? OperatorID { get => _meal.OperatorID; }

    private string _mealName = string.Empty;
    [Required(ErrorMessage = "Meal.MealName is required.")]
    [StringLength(100, ErrorMessage = "Meal.MealName must be less than 100 characters.")]
    public string MealName
    {
        get => _mealName;
        set
        {
            if (_mealName != value)
            {
                _mealName = value;
                OnPropertyChanged();
                _meal.MealName = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private string _description = string.Empty;
    [StringLength(500, ErrorMessage = "Meal.Description must be less than 500 characters.")]
    public string Description
    {
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged();
                _meal.Description = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private decimal _mealPrice = 0m;
    [Required(ErrorMessage = "Meal.MealPrice is required.")]
    [Range(0.01, 1000, ErrorMessage = "Meal.MealPrice must be greater than PHP0.01 and less than PHP1M.")]
    public decimal MealPrice
    {
        get => _mealPrice;
        set
        {
            if (_mealPrice != value)
            {
                _mealPrice = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedPrice));
                _meal.MealPrice = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private int _stockQuantity = 0;
    [Required(ErrorMessage = "Meal.StockQuantity is required.")]
    [Range(0, int.MaxValue, ErrorMessage = "Meal.StockQuantity cannot be negative.")]
    public int StockQuantity
    {
        get => _stockQuantity;
        set
        {
            if (_stockQuantity != value)
            {
                _stockQuantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StockStatus));
                OnPropertyChanged(nameof(IsAvailable));
                OnPropertyChanged(nameof(AvailabilityColor));
                OnPropertyChanged(nameof(StockQuantityText));
                OnPropertyChanged(nameof(LowStockWarningVisibility));
                _meal.StockQuantity = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private int _minOrderQuantity = 1;
    [Required(ErrorMessage = "Meal.MinOrderQuantity is required.")]
    [Range(1, 1000, ErrorMessage = "Meal.MinOrderQuantity must be between 1 and 1000.")]
    public int MinOrderQuantity
    {
        get => _minOrderQuantity;
        set
        {
            if (_minOrderQuantity != value)
            {
                _minOrderQuantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAvailable));
                OnPropertyChanged(nameof(AvailabilityColor));
                OnPropertyChanged(nameof(MinMaxOrderText));
                _meal.MinOrderQuantity = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private List<MealTagType> _mealTags = new List<MealTagType>();
    public List<MealTagType> MealTags
    {
        get => _mealTags;
        set
        {
            if (_mealTags != value)
            {
                _mealTags = value;
                OnPropertyChanged();
                _meal.MealTags = value;
                _hasChanges = true;
            }
        }
    }

    private DeliveryType _deliveryType;
    [Required(ErrorMessage = "Delivery type is required.")]
    public DeliveryType DeliveryType
    {
        get => _deliveryType;
        set
        {
            if (_deliveryType != value)
            {
                _deliveryType = value;
                OnPropertyChanged();
                _meal.DeliveryType = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private int _operatorId;
    public int OperatorId
    {
        get => _operatorId;
        set
        {
            if (_operatorId != value)
            {
                _operatorId = value;
                OnPropertyChanged();
                _meal.OperatorID = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private int _lastModifiedByOperatorID;
    [Required]
    public int LastModifiedByOperatorID
    {
        get => _lastModifiedByOperatorID;
        set
        {
            if (_lastModifiedByOperatorID != value)
            {
                _lastModifiedByOperatorID = value;
                OnPropertyChanged();
                _meal.LastModifiedByOperatorID = value;
                _hasChanges = true;
                ValidateAllProperties();
            }
        }
    }

    private string _imageSourceString = string.Empty;
    public string ImageSourceString
    {
        get => _imageSourceString;
        set
        {
            if (_imageSourceString != value)
            {
                _imageSourceString = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MealImage));
                _meal.ImageSourceString = value;
                _hasChanges = true;
            }
        }
    }

    private DateTime _creationDate;
    [Required]
    public DateTime CreationDate
    {
        get => _creationDate;
        set
        {
            if (_creationDate != value)
            {
                _creationDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RelativeCreationTime));
                _meal.CreationDate = value;
            }
        }
    }

    private DateTime _lastModifiedDate;
    [Required]
    public DateTime LastModifiedDate
    {
        get => _lastModifiedDate;
        set
        {
            if (_lastModifiedDate != value)
            {
                _lastModifiedDate = value;
                OnPropertyChanged();
                _meal.LastModifiedDate = value;
                _hasChanges = true;
            }
        }
    }

    private AppUser? _modifiedByOperator;
    public AppUser? ModifiedByOperator
    {
        get => _modifiedByOperator;
        set
        {
            if (_modifiedByOperator != value)
            {
                _modifiedByOperator = value;
                OnPropertyChanged();
                _meal.ModifiedByOperator = value;
                _hasChanges = true;
            }
        }
    }

    // Helper viewmodels
    public BitmapImage? MealImage
    {
        get
        {
            if (!string.IsNullOrEmpty(_imageSourceString))
            {
                return new BitmapImage(new Uri(_imageSourceString));
            }
            return null;
        }
    }

    public string FormattedPrice => MealPrice.ToString("C");
    public bool IsAvailable => StockQuantity >= MinOrderQuantity;
    public string AvailabilityColor => IsAvailable ? "Green" : "Red";
    public string StockStatus
    {
        get
        {
            if (StockQuantity == 0) return "Out of Stock";
            if (StockQuantity <= 5) return "Low Stock";
            return "In Stock";
        }
    }

    public string RelativeCreationTime
    {
        get
        {
            var timeSpan = DateTime.UtcNow - CreationDate;
            if (timeSpan.TotalDays < 1) return "Today";
            if (timeSpan.TotalDays < 7) return $"{timeSpan.Days} days ago";
            if (timeSpan.TotalDays < 30) return $"{timeSpan.Days / 7} weeks ago";
            return $"{timeSpan.Days / 30} months ago";
        }
    }

    public string StockQuantityText => $"{StockQuantity} units";
    public string MinMaxOrderText => $"Min: {MinOrderQuantity} units";
    public Visibility LowStockWarningVisibility => StockQuantity <= 5 ? Visibility.Visible : Visibility.Collapsed;
    public bool IsValid => !HasErrors;

    // RELAY COMMANDS
    [RelayCommand]
    public async Task CreateCommand()
    {
        ValidateAllProperties();
        if (IsValid && _hasChanges && MealID == null)
        {
            if (_mealRepository != null)
            {
                await _mealRepository.CreateMealAsync(_meal);
                _hasChanges = false;
            }
        }
    }

    [RelayCommand]
    public async Task UpdateCommand()
    {
        ValidateAllProperties();
        if (IsValid && _hasChanges && MealID.HasValue)
        {
            if (_mealRepository != null)
            {
                await _mealRepository.UpdateMealAsync(_meal);
                _hasChanges = false;
            }
        }
    }

    [RelayCommand]
    public async Task DeleteCommand()
    {
        if (MealID.HasValue && _mealRepository != null)
        {
            await _mealRepository.DeleteMealAsync(_meal.MealID);
            _parent?.DeleteMeal(this);
        }
    }

    // PARENT DEPENDENCY INJECTION
    private readonly AdminMealListViewModel? _parent;

    public RelayCommand DeleteCommandParent => new RelayCommand(() => _parent?.DeleteMeal(this));
    public RelayCommand RequestUpdateCommand => new RelayCommand(() => _parent?.UpdateSelectedMealCommand.Execute(null));

    // INITIAL DATA LOADING (via constructor injection)
    public MealViewModel(Meal meal, AdminMealListViewModel? parent)
    {
        _meal = meal ?? new Meal();
        _parent = parent;
        _mealRepository = parent?.MealRepository;
        _hasChanges = false;

        _mealName = _meal.MealName ?? string.Empty;
        _description = _meal.Description ?? string.Empty;
        _mealPrice = _meal.MealPrice;
        _stockQuantity = _meal.StockQuantity;
        _minOrderQuantity = _meal.MinOrderQuantity;
        _mealTags = _meal.MealTags?.ToList() ?? new List<MealTagType>();
        _deliveryType = _meal.DeliveryType;
        _operatorId = _meal.OperatorID;
        _lastModifiedByOperatorID = _meal.LastModifiedByOperatorID;
        _imageSourceString = _meal.ImageSourceString ?? string.Empty;
        _creationDate = _meal.CreationDate;
        _lastModifiedDate = _meal.LastModifiedDate;
        _modifiedByOperator = _meal.ModifiedByOperator;

        ValidateAllProperties();
    }

    public MealViewModel() : this(new Meal(), null) { }
}

public partial class MealProductViewModel : ObservableValidator
{
    private MealProduct _mealProduct = new();
    public MealProduct GetMealProduct() => _mealProduct;

    private int _mealProductID;
    public int MealProductID
    {
        get => _mealProductID;
        set
        {
            if (_mealProductID != value)
            {
                _mealProductID = value;
                OnPropertyChanged();
                _mealProduct.MealProductID = value;
            }
        }
    }

    private int _ownerID;
    public int OwnerID
    {
        get => _ownerID;
        set
        {
            if (_ownerID != value)
            {
                _ownerID = value;
                OnPropertyChanged();
                _mealProduct.OwnerID = value;
                ValidateAllProperties();
            }
        }
    }

    private int? _promoID;
    public int? PromoID
    {
        get => _promoID;
        set
        {
            if (_promoID != value)
            {
                _promoID = value;
                OnPropertyChanged();
                _mealProduct.PromoID = value;
                ValidateAllProperties();
            }
        }
    }

    //mao ba ni tong di na need? 
    private AppUser? _owner;
    public AppUser? Owner
    {
        get => _owner;
        set
        {
            if (_owner != value)
            {
                _owner = value;
                OnPropertyChanged();
                _mealProduct.Owner = value;
                ValidateAllProperties();
            }
        }
    }

    private ICollection<MealProductItem> _mealProductItems = [];
    public ICollection<MealProductItem> MealProductItems
    {
        get => _mealProductItems;
        set
        {
            if (_mealProductItems != value)
            {
                _mealProductItems = value;
                OnPropertyChanged();
                _mealProduct.MealProductItems = value;
                ValidateAllProperties();
            }
        }
    }

    private Promo? _promo;
    public Promo? Promo
    {
        get => _promo;
        set
        {
            if (_promo != value)
            {
                _promo = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProductBasePrice));
                OnPropertyChanged(nameof(DiscountAmount));
                OnPropertyChanged(nameof(FinalPrice));
                _mealProduct.Promo = value;
                ValidateAllProperties();
            }
        }
    }

    private string _productName = string.Empty;
    [Required]
    [StringLength(100)]
    public string ProductName
    {
        get => _productName;
        set
        {
            if (_productName != value)
            {
                _productName = value;
                OnPropertyChanged();
                _mealProduct.ProductName = value;
                ValidateAllProperties();
            }
        }
    }

    private string _productDescription = string.Empty;
    [StringLength(100)]
    public string ProductDescription
    {
        get => _productDescription;
        set
        {
            if (_productDescription != value)
            {
                _productDescription = value;
                OnPropertyChanged();
                _mealProduct.ProductDescription = value;
                ValidateAllProperties();
            }
        }
    }

    // Computed properties
    public decimal ProductBasePrice => MealProductItems.Sum(item => item.ItemPrice);
    public decimal DiscountAmount => ProductBasePrice - FinalPrice;
    public decimal FinalPrice => Promo?.ApplyDiscount(ProductBasePrice) ?? ProductBasePrice;
    public bool IsValid => !HasErrors;

    public MealProductViewModel(MealProduct mealProduct)
    {
        _mealProduct = mealProduct ?? new MealProduct();
        _mealProductID = _mealProduct.MealProductID;
        _ownerID = _mealProduct.OwnerID;
        _promoID = _mealProduct.PromoID;
        _owner = _mealProduct.Owner;
        _mealProductItems = _mealProduct.MealProductItems ?? [];
        _promo = _mealProduct.Promo;
        _productName = _mealProduct.ProductName ?? string.Empty;
        _productDescription = _mealProduct.ProductDescription ?? string.Empty;

        ValidateAllProperties();
    }

    public MealProductViewModel() : this(new MealProduct()) { }
}

public partial class MealProductItemViewModel : ObservableValidator
{
    private MealProductItem _mealProductItem = new();
    public MealProductItem GetMealProductItem() => _mealProductItem;

    private int _mealID;
    public int MealID
    {
        get => _mealID;
        set
        {
            if (_mealID != value)
            {
                _mealID = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemPrice));
                _mealProductItem.MealID = value;
                ValidateAllProperties();
            }
        }
    }

    private Meal? _meal;
    public Meal? Meal
    {
        get => _meal;
        set
        {
            if (_meal != value)
            {
                _meal = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemPrice));
                _mealProductItem.Meal = value;
                ValidateAllProperties();
            }
        }
    }

    private int _quantity = 1;
    [Range(1, 100)]
    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemPrice));
                _mealProductItem.Quantity = value;
                ValidateAllProperties();
            }
        }
    }

    private string _requestDescription = string.Empty;
    [StringLength(100)]
    public string RequestDescription
    {
        get => _requestDescription;
        set
        {
            if (_requestDescription != value)
            {
                _requestDescription = value;
                OnPropertyChanged();
                _mealProductItem.RequestDescription = value;
                ValidateAllProperties();
            }
        }
    }

    public decimal ItemPrice => Meal?.MealPrice * Quantity ?? 0m;
    public bool IsValid => !HasErrors;

    public MealProductItemViewModel(MealProductItem mealProductItem)
    {
        _mealProductItem = mealProductItem ?? new MealProductItem();
        _mealID = _mealProductItem.MealID;
        _meal = _mealProductItem.Meal;
        _quantity = _mealProductItem.Quantity;
        _requestDescription = _mealProductItem.RequestDescription ?? string.Empty;

        ValidateAllProperties();
    }

    public MealProductItemViewModel() : this(new MealProductItem()) { }
}