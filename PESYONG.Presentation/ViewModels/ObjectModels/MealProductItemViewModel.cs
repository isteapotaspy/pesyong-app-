using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PESYONG.ApplicationLogic.ViewModels.ObjectModels;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Meals.MealProduct;

namespace PESYONG.Presentation.ViewModels.PageModels;

public partial class MealProductItemViewModel : ObservableValidator
{
    [ObservableProperty]
    private int mealID;

    [ObservableProperty]
    private MealViewModel? meal;

    [ObservableProperty]
    [Range(1, 100)]
    private int quantity = 1;

    [ObservableProperty]
    [StringLength(100)]
    private string? requestDescription;

    [ObservableProperty]
    private decimal itemPrice;

    [JsonIgnore]
    public IRelayCommand IncrementQuantityCommand { get; }
    [JsonIgnore]
    public IRelayCommand DecrementQuantityCommand { get; }

    public MealProductItemViewModel()
    {
        IncrementQuantityCommand = new RelayCommand(IncrementQuantity);
        DecrementQuantityCommand = new RelayCommand(DecrementQuantity);

        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(Quantity) || e.PropertyName == nameof(Meal))
            {
                RecalculatePrice();
            }
        };
    }

    public static MealProductItemViewModel CreateFromEntity(MealProductItem item)
    {
        return new MealProductItemViewModel
        {
            MealID = item.MealID,
            Quantity = item.Quantity,
            RequestDescription = item.RequestDescription,
            Meal = item.Meal != null ? MealViewModel.CreateFromEntity(item.Meal, null) : null
        };
    }

    public MealProductItem ToEntity()
    {
        return new MealProductItem
        {
            MealID = MealID,
            Quantity = Quantity,
            RequestDescription = string.IsNullOrWhiteSpace(RequestDescription) ? null : RequestDescription,
            Meal = Meal?.ToEntity()
        };
    }

    private void IncrementQuantity()
    {
        if (Quantity < 100)
        {
            Quantity++;
        }
    }

    private void DecrementQuantity()
    {
        if (Quantity > 1)
        {
            Quantity--;
        }
    }

    private void RecalculatePrice()
    {
        ItemPrice = (Meal?.MealPrice ?? 0m) * Quantity;
    }
}
