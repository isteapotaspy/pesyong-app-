using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens.Experimental;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.ApplicationLogic.ViewModels;
using PESYONG.Domain.Entities;
using PESYONG.Domain.Entities.Financial.Promos;
using PESYONG.Domain.Entities.Meals.MealProduct;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Entities.Users.Identity;

namespace PESYONG.Presentation.ViewModels.PageModels;

public partial class MealProductViewModel : ObservableValidator
{
    private MealProductRepository _mealProductRepository;

    [ObservableProperty]
    private int? mealProductID;

    [ObservableProperty]
    private int? ownerID;

    [ObservableProperty]
    private int? promoID;

    [ObservableProperty]
    private AppUser? owner;

    [ObservableProperty]
    private bool isCateringPackage;

    [Required]
    [ObservableProperty]
    private string productName = string.Empty;

    [ObservableProperty]
    private string? productDescription;

    [ObservableProperty]
    private ObservableCollection<MealProductItemViewModel> mealProductItems = new();

    [ObservableProperty]
    private bool hasValidationErrors;

    [ObservableProperty]
    private ObservableCollection<string> validationErrors = new();

    // Computed properties
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DiscountAmount))]
    [NotifyPropertyChangedFor(nameof(FinalPrice))]
    private decimal productBasePrice;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FinalPrice))]
    private decimal discountAmount;

    [ObservableProperty]
    private decimal finalPrice;

    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }
    public IRelayCommand AddItemCommand { get; }
    public IRelayCommand RemoveItemCommand { get; }

    public MealProductViewModel()
    {
        _mealProductRepository = App.Instance.Services.GetRequiredService<MealProductRepository>();

        SaveCommand = new AsyncRelayCommand(SaveMealProductAsync, CanSaveMealProduct);
        LoadCommand = new AsyncRelayCommand(LoadMealProductAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteMealProductAsync);
        AddItemCommand = new RelayCommand(AddMealProductItem);
        RemoveItemCommand = new RelayCommand<MealProductItemViewModel>(RemoveMealProductItem);

        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName != nameof(HasValidationErrors) &&
                e.PropertyName != nameof(ValidationErrors))
            {
                Validate();
                SaveCommand.NotifyCanExecuteChanged();

                // Recalculate prices when items change
                if (e.PropertyName == nameof(MealProductItems) ||
                    e.PropertyName == nameof(Promo))
                {
                    RecalculatePrices();
                }
            }
        };

        MealProductItems.CollectionChanged += (s, e) => RecalculatePrices();
    }

    public static MealProductViewModel CreateFromEntity(MealProduct mealProduct)
    {
        var vm = new MealProductViewModel();
        vm.LoadFromEntity(mealProduct);
        return vm;
    }

    public void LoadFromEntity(MealProduct mealProduct)
    {
        MealProductID = mealProduct.MealProductID;
        OwnerID = mealProduct.OwnerID;
        Owner = mealProduct.Owner;
        IsCateringPackage = mealProduct.IsCateringPackage;
        ProductName = mealProduct.ProductName;
        ProductDescription = mealProduct.ProductDescription;

        MealProductItems.Clear();
        if (mealProduct.MealProductItems != null)
        {
            foreach (var item in mealProduct.MealProductItems)
            {
                MealProductItems.Add(MealProductItemViewModel.CreateFromEntity(item));
            }
        }

        RecalculatePrices();
    }

    public MealProduct ToEntity()
    {
        return new MealProduct
        {
            MealProductID = (int)MealProductID,
            OwnerID = OwnerID ?? 0,
            PromoID = PromoID,
            Owner = Owner,
            IsCateringPackage = IsCateringPackage,
            ProductName = ProductName,
            ProductDescription = string.IsNullOrWhiteSpace(ProductDescription) ? null : ProductDescription,
            MealProductItems = MealProductItems.Select(item => item.ToEntity()).ToList(),
        };
    }

    private bool CanSaveMealProduct() => !HasValidationErrors;

    private void AddMealProductItem()
    {
        MealProductItems.Add(new MealProductItemViewModel());
    }

    private void RemoveMealProductItem(MealProductItemViewModel? item)
    {
        if (item != null && MealProductItems.Contains(item))
        {
            MealProductItems.Remove(item);
        }
    }

    private void RecalculatePrices()
    {
        ProductBasePrice = MealProductItems.Sum(item => item.ItemPrice);
    }

    public void ClearMealProductViewModel()
    {
        MealProductID = null;
        OwnerID = null;
        PromoID = null;
        Owner = null;
        IsCateringPackage = false;
        ProductName = string.Empty;
        ProductDescription = null;
        MealProductItems.Clear();
        RecalculatePrices();
    }

    private async Task SaveMealProductAsync()
    {
        try
        {
            // var entity = ToEntity();
            // if (MealProductID.HasValue)
            // {
            //     await _mealRepository.UpdateMealProductAsync(entity);
            // }
            // else
            // {
            //     await _mealRepository.CreateMealProductAsync(entity);
            // }
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", "An error occurred while saving meal product", "OK");
        }
    }

    private async Task LoadMealProductAsync()
    {
        try
        {
            // var mealProduct = await _mealRepository.GetMealProductByIdAsync(MealProductID.Value);
            // if (mealProduct != null)
            // {
            //     LoadFromEntity(mealProduct);
            // }
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", "Failed to load meal product", "OK");
        }
    }

    private async Task DeleteMealProductAsync()
    {
        bool confirmed = true;

        if (confirmed)
        {
            try
            {
                // Implementation would depend on your repository methods
                // await _mealRepository.DeleteMealProductAsync(MealProductID.Value);
            }
            catch (Exception ex)
            {
                ShowEventOnDebugConsole("Error", "An error occurred while deleting meal product", "OK");
            }
        }
    }

    private void Validate()
    {
        var entity = ToEntity();
        var errors = entity.GetValidationErrors().ToList();

        ValidationErrors.Clear();
        foreach (var error in errors)
        {
            ValidationErrors.Add(error);
        }

        HasValidationErrors = errors.Any();
    }

    private void ShowEventOnDebugConsole(string title, string message, string buttonText)
    {
        Debug.Write($"[{title}] {buttonText} : {message}");
    }
}
