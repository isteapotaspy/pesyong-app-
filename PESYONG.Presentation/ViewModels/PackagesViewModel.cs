using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using PESYONG.ApplicationLogic.DTOs;
using PESYONG.ApplicationLogic.Services;
using PESYONG.Presentation.Views;
using PESYONG.Presentation.Views.Customer;

namespace PESYONG.Presentation.ViewModels;
public partial class PackagesViewModel : ObservableObject
{
    private readonly CateringService _service;
    private readonly CartService _cartService;

    [ObservableProperty]
    public partial ObservableCollection<MealSelectionDto> Viands { get; set; } = new();

    // Change to PackageDisplayDto instead of MealProduct to avoid read-only issues
    [ObservableProperty]
    private ObservableCollection<PackageDisplayDto> _availablePackages = new();

    [ObservableProperty]
    private decimal _discount = 0;

    // Computed Properties for UI Data Binding
    public IEnumerable<MealSelectionDto> SelectedViands => Viands.Where(x => x.IsSelected);
    public decimal BasePrice => SelectedViands.Sum(x => x.Price);
    public decimal FinalPrice => BasePrice - Discount;

    public PackagesViewModel(CateringService service, CartService cartService)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));

        LoadData();
        LoadPackages();
    }

    private void LoadData()
    {
        var data = _service.GetAvailableViands();
        Viands = new ObservableCollection<MealSelectionDto>(data);

        foreach (var item in Viands)
        {
            item.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MealSelectionDto.IsSelected))
                {
                    OnPropertyChanged(nameof(SelectedViands));
                    OnPropertyChanged(nameof(BasePrice));
                    OnPropertyChanged(nameof(FinalPrice));
                    FinalizeOrderCommand.NotifyCanExecuteChanged();
                }
            };
        }
    }

    private void LoadPackages()
    {
        // Create package DTOs using the positional constructor signatures
        AvailablePackages = new ObservableCollection<PackageDisplayDto>
        {
            new PackageDisplayDto(1, "Fiesta Package", "A complete set for 20 pax including rice and dessert.", 3500.00m),
            new PackageDisplayDto(2, "Deluxe Package", "Premium selection for 30 pax with dessert and drinks.", 5500.00m),
            new PackageDisplayDto(3, "Basic Package", "Simple package for 10 pax.", 1800.00m)
        };
    }

    [RelayCommand]
    private void AddToCart(PackageDisplayDto package)
    {
        if (package == null) return;

        try
        {
            // Create a simplified cart item
            var cartItem = new
            {
                ProductId = package.Id,
                ProductName = package.Name,
                Price = package.Price,
                Quantity = 1
            };

            // Use the actual CartService method - you need to check what methods are available
            // Option 1: If there's an AddToCart method
            _cartService.AddToCart(package.Id, package.Price, 1);

            // Option 2: If it expects a different type, you might need to create the proper entity
            // var orderMealProduct = new OrderMealProduct
            // {
            //     MealProductID = package.Id,
            //     ItemPrice = package.Price,
            //     MealProductOrderQty = 1
            // };
            // _cartService.AddItem(orderMealProduct);

            System.Diagnostics.Debug.WriteLine($"Added {package.Name} to Cart. Price: {package.Price:C}");

            // Optional: Show a visual feedback
            ShowNotification($"Added {package.Name} to cart");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding to cart: {ex.Message}");
            ShowNotification($"Error: {ex.Message}");
        }
    }

    private void ShowNotification(string message)
    {
        // Implement proper notification (you might want to use a service for this)
        System.Diagnostics.Debug.WriteLine(message);
    }

    [RelayCommand(CanExecute = nameof(CanFinalize))]
    private void FinalizeOrder()
    {
        try
        {
            var selected = SelectedViands.ToList();
            var order = _service.CreateOrderFromSelection(selected, 1);

            if (App.Current.MainWindow.Content is Frame rootFrame)
            {
                rootFrame.Navigate(typeof(CheckoutPage), order);
            }
        }
        catch (Exception ex)
        {
            ShowNotification($"Error finalizing order: {ex.Message}");
        }
    }

    private bool CanFinalize() => SelectedViands.Any() && SelectedViands.Count() <= 8;
}