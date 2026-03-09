using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Meals.MealProduct;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

public sealed class MealProductItemViewModel : INotifyPropertyChanged, IDataErrorInfo
{
    private int _mealId;
    private string _mealName = string.Empty;
    private int _quantity = 1;
    private string? _requestDescription;
    private decimal _unitPrice;
    private Meal? _mealReference;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int MealID
    {
        get => _mealId;
        set
        {
            if (SetProperty(ref _mealId, value))
            {
                OnPropertyChanged(nameof(ItemPrice));
                OnPropertyChanged(nameof(FormattedItemPrice));
            }
        }
    }

    public string MealName
    {
        get => _mealName;
        set => SetProperty(ref _mealName, value);
    }

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (SetProperty(ref _quantity, value))
            {
                OnPropertyChanged(nameof(ItemPrice));
                OnPropertyChanged(nameof(FormattedItemPrice));
            }
        }
    }

    public string? RequestDescription
    {
        get => _requestDescription;
        set => SetProperty(ref _requestDescription, value);
    }

    public decimal UnitPrice
    {
        get => _unitPrice;
        set
        {
            if (SetProperty(ref _unitPrice, value))
            {
                OnPropertyChanged(nameof(ItemPrice));
                OnPropertyChanged(nameof(FormattedUnitPrice));
                OnPropertyChanged(nameof(FormattedItemPrice));
            }
        }
    }

    public Meal? MealReference
    {
        get => _mealReference;
        set => SetProperty(ref _mealReference, value);
    }

    public decimal ItemPrice => UnitPrice * Quantity;

    public string FormattedUnitPrice => $"PHP {UnitPrice:N2}";

    public string FormattedItemPrice => $"PHP {ItemPrice:N2}";

    public string Error => string.Empty;

    public string this[string columnName]
    {
        get
        {
            return columnName switch
            {
                nameof(MealID) when MealID <= 0 => "Meal is required.",
                nameof(Quantity) when Quantity < 1 => "Quantity must be at least 1.",
                nameof(Quantity) when Quantity > 100 => "Quantity must not exceed 100.",
                nameof(RequestDescription) when !string.IsNullOrWhiteSpace(RequestDescription) && RequestDescription.Length > 100
                    => "Request description must not exceed 100 characters.",
                _ => string.Empty
            };
        }
    }

    public bool IsValid()
    {
        return string.IsNullOrWhiteSpace(this[nameof(MealID)])
            && string.IsNullOrWhiteSpace(this[nameof(Quantity)])
            && string.IsNullOrWhiteSpace(this[nameof(RequestDescription)]);
    }

    public MealProductItem ToEntity()
    {
        return new MealProductItem
        {
            MealID = MealID,
            Quantity = Quantity,
            RequestDescription = string.IsNullOrWhiteSpace(RequestDescription)
                ? null
                : RequestDescription.Trim(),
            Meal = null
        };
    }

    public static MealProductItemViewModel CreateFromEntity(MealProductItem entity)
    {
        return new MealProductItemViewModel
        {
            MealID = entity.MealID,
            MealName = entity.Meal?.MealName ?? $"Meal #{entity.MealID}",
            Quantity = entity.Quantity,
            RequestDescription = entity.RequestDescription,
            UnitPrice = entity.Meal?.MealPrice ?? 0m,
            MealReference = entity.Meal
        };
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}