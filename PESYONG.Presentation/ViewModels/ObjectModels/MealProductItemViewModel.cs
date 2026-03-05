using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Meals.MealProduct;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

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
