using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PESYONG.Domain.Entities.Financial.Promos;
using PESYONG.Domain.Entities.Meals.MealProduct;
using PESYONG.Domain.Entities.Users.Identity;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

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
