using PESYONG.Domain.Entities.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

public class OrderMealProductViewModel : INotifyPropertyChanged, IDataErrorInfo
{
    private Guid _orderID;
    private int _mealProductID;
    private string _mealProductName = string.Empty;
    private decimal _itemPrice;
    private int _mealProductOrderQty = 1;
    private string _validationMessage = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public OrderMealProductViewModel()
    {
    }

    public OrderMealProductViewModel(OrderMealProduct entity)
    {
        if (entity == null)
            return;

        LoadFromEntity(entity);
    }

    public Guid OrderID
    {
        get => _orderID;
        set => SetProperty(ref _orderID, value);
    }

    public int MealProductID
    {
        get => _mealProductID;
        set => SetProperty(ref _mealProductID, value);
    }

    public string MealProductName
    {
        get => _mealProductName;
        set => SetProperty(ref _mealProductName, value);
    }

    [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Item price must be greater than zero.")]
    public decimal ItemPrice
    {
        get => _itemPrice;
        set
        {
            if (SetProperty(ref _itemPrice, value))
            {
                OnPropertyChanged(nameof(FormattedItemPrice));
                OnPropertyChanged(nameof(SubTotal));
                OnPropertyChanged(nameof(FormattedSubTotal));
            }
        }
    }

    [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000.")]
    public int MealProductOrderQty
    {
        get => _mealProductOrderQty;
        set
        {
            if (SetProperty(ref _mealProductOrderQty, value))
            {
                OnPropertyChanged(nameof(SubTotal));
                OnPropertyChanged(nameof(FormattedSubTotal));
            }
        }
    }

    public decimal SubTotal => MealProductOrderQty * ItemPrice;

    public string FormattedItemPrice => $"PHP {ItemPrice:N2}";

    public string FormattedSubTotal => $"PHP {SubTotal:N2}";

    public string ValidationMessage
    {
        get => _validationMessage;
        set => SetProperty(ref _validationMessage, value);
    }

    public string Error => string.Empty;

    public string this[string columnName]
    {
        get
        {
            try
            {
                var context = new ValidationContext(this) { MemberName = columnName };
                var results = new List<ValidationResult>();

                var property = GetType().GetProperty(columnName);
                if (property == null)
                    return string.Empty;

                var value = property.GetValue(this);
                var valid = Validator.TryValidateProperty(value, context, results);

                return valid
                    ? string.Empty
                    : results.FirstOrDefault()?.ErrorMessage ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    public void LoadFromEntity(OrderMealProduct entity)
    {
        OrderID = entity.OrderID;
        MealProductID = entity.MealProductID;
        MealProductName = entity.MealProduct?.ProductName ?? $"Meal Product #{entity.MealProductID}";
        ItemPrice = entity.ItemPrice;
        MealProductOrderQty = entity.MealProductOrderQty;
    }

    public static OrderMealProductViewModel CreateFromEntity(OrderMealProduct entity)
    {
        var vm = new OrderMealProductViewModel();
        vm.LoadFromEntity(entity);
        return vm;
    }

    public OrderMealProduct ToEntity()
    {
        return new OrderMealProduct
        {
            OrderID = OrderID,
            MealProductID = MealProductID,
            ItemPrice = ItemPrice,
            MealProductOrderQty = MealProductOrderQty
        };
    }

    public bool ValidateAll()
    {
        try
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(this);
            var isValid = Validator.TryValidateObject(this, context, results, true);

            ValidationMessage = string.Join(
                Environment.NewLine,
                results.Select(x => x.ErrorMessage)
                       .Where(x => !string.IsNullOrWhiteSpace(x)));

            return isValid;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OrderMealProduct validation failed: {ex}");
            ValidationMessage = "Item validation failed.";
            return false;
        }
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}