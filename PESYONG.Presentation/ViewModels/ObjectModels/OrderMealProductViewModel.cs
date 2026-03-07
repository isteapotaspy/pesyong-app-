using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PESYONG.Domain.Entities.Meals.MealProduct;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Presentation.ViewModels.PageModels;
using PESYONG.Presentation.Views.Admin.Meals;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

public partial class OrderMealProductViewModel : ObservableValidator
{
    [ObservableProperty]
    [Required(ErrorMessage = "Order ID is required")]
    private Guid _orderID;

    [ObservableProperty]
    private OrderViewModel? _order;

    [ObservableProperty]
    [Required(ErrorMessage = "Meal product ID is required")]
    private int _mealProductID;

    [ObservableProperty]
    private MealProductViewModel? _mealProduct;

    [ObservableProperty]
    [Required(ErrorMessage = "Item price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Item price must be greater than 0")]
    [NotifyPropertyChangedFor(nameof(SubTotal))]
    [NotifyDataErrorInfo]
    private decimal _itemPrice;

    [ObservableProperty]
    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
    [NotifyPropertyChangedFor(nameof(SubTotal))]
    [NotifyDataErrorInfo]
    private int _mealProductOrderQty = 1;

    public OrderMealProductViewModel()
    {
        ValidateAllProperties();
    }


    public OrderMealProductViewModel(OrderMealProduct entity)
    {
        if (entity != null)
        {
            OrderID = entity.OrderID;
            MealProductID = entity.MealProductID;
            ItemPrice = entity.ItemPrice;
            MealProductOrderQty = entity.MealProductOrderQty;

            // Convert MealProduct to MealProductViewModel if it exists
            if (entity.MealProduct != null)
            {
                MealProduct = MealProductViewModel.CreateFromEntity(entity.MealProduct);
            }
        }

        ValidateAllProperties();
    }


    // Computed properties
    public decimal SubTotal => MealProductOrderQty * ItemPrice;

    public bool HasMealProduct => MealProduct != null;

    //public string ProductName => MealProduct?.Name ?? "Unknown Product";

    // Entity Mappers
    public static OrderMealProductViewModel FromEntity(OrderMealProduct entity)
    {
        if (entity == null)
            return new OrderMealProductViewModel();

        return new OrderMealProductViewModel
        {
            OrderID = entity.OrderID,
            MealProductID = entity.MealProductID,
            ItemPrice = entity.ItemPrice,
            MealProductOrderQty = entity.MealProductOrderQty
        };
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

    // Validation helpers
    public string GetErrorMessages()
    {
        if (!HasErrors) return string.Empty;

        var errors = GetErrors()
            .Select(error => $"{error.MemberNames.FirstOrDefault()}: {error.ErrorMessage}");

        return string.Join(Environment.NewLine, errors);
    }

    // Clone method
    public OrderMealProductViewModel Clone()
    {
        return new OrderMealProductViewModel
        {
            OrderID = OrderID,
            MealProductID = MealProductID,
            ItemPrice = ItemPrice,
            MealProductOrderQty = MealProductOrderQty,
            //MealProduct = MealProduct?.Clone()
        };
    }

    // Update from meal product
    public void UpdateFromMealProduct(MealProductViewModel mealProduct)
    {
        if (mealProduct != null)
        {
            MealProduct = mealProduct;
            MealProductID = (int)mealProduct.MealProductID;
            ItemPrice = mealProduct.FinalPrice; // Use current price
        }
    }

    // Reset method
    public void Reset()
    {
        OrderID = Guid.Empty;
        MealProductID = 0;
        ItemPrice = 0;
        MealProductOrderQty = 1;
        MealProduct = null;

        ClearErrors();
    }
}