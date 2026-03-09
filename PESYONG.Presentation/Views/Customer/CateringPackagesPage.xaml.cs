using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.ApplicationLogic.Services;
using PESYONG.Domain.Entities;
using PESYONG.Domain.Entities.Financial.AcknowledgementReceipts;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Meals.MealProduct;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Entities.Users.Identity;
using PESYONG.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace PESYONG.Presentation.Views.Customer
{
    public sealed partial class CateringPackagesPage : Page
    {
        private ObservableCollection<CateringPackageCardViewModel> Packages { get; set; } = new();
        private ObservableCollection<Meal> AvailableViands { get; set; } = new();
        private List<Meal> SelectedViands { get; set; } = new();
        private CateringPackageCardViewModel? CurrentSelectedPackage { get; set; }
        private int RequiredViandCount { get; set; } = 8;

        private readonly CartService _cartService;
        private readonly MealProductRepository _mealProductRepository;
        private readonly MealRepository _mealRepository;

        public CateringPackagesPage()
        {
            InitializeComponent();

            _cartService = CartService.Instance;
            _mealProductRepository = App.Current.Services.GetRequiredService<MealProductRepository>();
            _mealRepository = App.Current.Services.GetRequiredService<MealRepository>();

            Loaded += CateringPackagesPage_Loaded;
            Unloaded += CateringPackagesPage_Unloaded;

            if (_cartService.Cart != null)
            {
                _cartService.Cart.CollectionChanged += Cart_CollectionChanged;
            }
        }

        private async void CateringPackagesPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPackagesAsync();
            await LoadAvailableViandsAsync();
            UpdateCartQuantities();
        }

        private void CateringPackagesPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_cartService.Cart != null)
            {
                _cartService.Cart.CollectionChanged -= Cart_CollectionChanged;
            }
        }

        private void Cart_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateCartQuantities();
        }

        private AppUser GetCurrentUser()
        {
            try
            {
                var currentUser = (App.Current as App)?.CurrentUser;

                if (currentUser == null)
                {
                    return new AppUser
                    {
                        Id = 1,
                        UserName = "test@email.com",
                        FirstName = "Test",
                        LastName = "User",
                        UserOrders = new List<Order>(),
                        UserMealProducts = new List<MealProduct>(),
                        UserReceipts = new List<AcknowledgementReceipt>()
                    };
                }

                currentUser.UserOrders ??= new List<Order>();
                currentUser.UserMealProducts ??= new List<MealProduct>();
                currentUser.UserReceipts ??= new List<AcknowledgementReceipt>();

                return currentUser;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetCurrentUser: {ex.Message}");
                return null!;
            }
        }

        private async Task LoadPackagesAsync()
        {
            try
            {
                var packageEntities = (await _mealProductRepository.GetAllMealProductsAsync())
                    .Where(p => p.IsCateringPackage && p.IsAvailable)
                    .OrderBy(p => p.MealProductID)
                    .ToList();

                Packages.Clear();

                foreach (var package in packageEntities)
                {
                    Packages.Add(new CateringPackageCardViewModel(package)
                    {
                        CartQuantity = GetCartQuantityForPackage(package.MealProductID)
                    });
                }

                PackagesItemsControl.ItemsSource = Packages;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadPackagesAsync: {ex}");
            }
        }

        private async Task LoadAvailableViandsAsync()
        {
            try
            {
                var meals = await _mealRepository.GetAllMealsAsync();

                AvailableViands.Clear();

                foreach (var meal in meals
                    .Where(m => m.MealID.HasValue)
                    .OrderBy(m => m.MealName))
                {
                    AvailableViands.Add(meal);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadAvailableViandsAsync: {ex}");
            }
        }

        private int GetCartQuantityForPackage(int packageId)
        {
            if (_cartService?.Cart == null)
                return 0;

            return _cartService.Cart
                .Where(c => c.ProductId == packageId && c.Type == "package")
                .Sum(c => c.Quantity);
        }

        private void UpdateCartQuantities()
        {
            if (Packages == null || _cartService?.Cart == null)
                return;

            foreach (var package in Packages)
            {
                package.CartQuantity = GetCartQuantityForPackage(package.MealProductID);
            }
        }

        private async void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is not Button button || button.DataContext is not CateringPackageCardViewModel package)
                    return;

                if (!package.IsCustomizable)
                {
                    AddToCart(package, null);
                    return;
                }

                CurrentSelectedPackage = package;
                SelectedViands.Clear();
                RequiredViandCount = package.PreferredViandCount > 0 ? package.PreferredViandCount : 8;

                DialogDescription.Text = $"Choose exactly {RequiredViandCount} viands for {package.ProductName}.";
                UpdateSelectedCount();

                await LoadAvailableViandsAsync();
                ViandsGrid.ItemsSource = AvailableViands;

                ViandSelectionDialog.XamlRoot = XamlRoot;
                await ViandSelectionDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AddToCart_Click: {ex.Message}");

                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Unable to open viand selection.",
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot
                };

                await dialog.ShowAsync();
            }
        }


        private void ViandCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is not CheckBox checkBox || checkBox.Tag == null)
                    return;

                int mealId = Convert.ToInt32(checkBox.Tag);
                var selectedMeal = AvailableViands.FirstOrDefault(m => (m.MealID ?? 0) == mealId);

                if (selectedMeal == null)
                    return;

                if (SelectedViands.Any(m => (m.MealID ?? 0) == mealId))
                    return;

                if (SelectedViands.Count >= RequiredViandCount)
                {
                    checkBox.IsChecked = false;

                    var warningDialog = new ContentDialog
                    {
                        Title = "Maximum Selection",
                        Content = $"You can only select up to {RequiredViandCount} viands.",
                        CloseButtonText = "OK",
                        XamlRoot = XamlRoot
                    };
                    _ = warningDialog.ShowAsync();
                    return;
                }

                SelectedViands.Add(selectedMeal);
                UpdateSelectedCount();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ViandCheckBox_Checked: {ex.Message}");
            }
        }

        private void ViandCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is not CheckBox checkBox || checkBox.Tag == null)
                    return;

                int mealId = Convert.ToInt32(checkBox.Tag);
                var selectedMeal = SelectedViands.FirstOrDefault(m => (m.MealID ?? 0) == mealId);

                if (selectedMeal != null)
                {
                    SelectedViands.Remove(selectedMeal);
                    UpdateSelectedCount();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ViandCheckBox_Unchecked: {ex.Message}");
            }
        }


        private void UpdateSelectedCount()
        {
            try
            {
                SelectedCountText.Text = $"Selected: {SelectedViands.Count} / {RequiredViandCount}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateSelectedCount: {ex.Message}");
            }
        }

        private void ViandSelectionDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                if (CurrentSelectedPackage == null)
                {
                    args.Cancel = true;
                    return;
                }

                if (SelectedViands.Count != RequiredViandCount)
                {
                    args.Cancel = true;

                    var errorDialog = new ContentDialog
                    {
                        Title = "Invalid Selection",
                        Content = $"Please select exactly {RequiredViandCount} viands.",
                        CloseButtonText = "OK",
                        XamlRoot = XamlRoot
                    };
                    _ = errorDialog.ShowAsync();
                    return;
                }

                AddToCart(CurrentSelectedPackage, SelectedViands.ToList());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ViandSelectionDialog_PrimaryButtonClick: {ex.Message}");
            }
        }

        private void AddToCart(CateringPackageCardViewModel package, List<Meal>? selectedViands)
        {
            try
            {
                decimal totalPrice = package.ProductBasePrice;
                string itemName = selectedViands != null && selectedViands.Any()
                    ? $"{package.ProductName} (Custom)"
                    : package.ProductName;
                var resolvedImageBytes = package.Package.ImageBytes
                ?? package.Package.MealProductItems?.FirstOrDefault()?.Meal?.ImageBytes;

                System.Diagnostics.Debug.WriteLine(
                $"Package '{package.ProductName}' image bytes length: {resolvedImageBytes?.Length ?? 0}");

                var cartItem = new CartItem
                {
                    Id = $"package_{package.MealProductID}_{Guid.NewGuid()}",
                    Name = itemName,
                    Price = (double)totalPrice,
                    Quantity = 1,
                    ImageBytes = resolvedImageBytes,
                    Type = "package",
                    ProductId = package.MealProductID,
                    Pax = package.PaxCount > 0 ? package.PaxCount : package.MealProductItems?.Count ?? 0,
                    CateringSelections = selectedViands?.Select(v => new CateringCartSelection
                    {
                        MealId = v.MealID ?? 0,
                        MealName = v.MealName,
                        Price = v.MealPrice
                    }).ToList()
                };

                _cartService.AddToCart(cartItem);

                UpdateCartQuantities();

                var successDialog = new ContentDialog
                {
                    Title = "Added to Cart!",
                    Content = $"{itemName} has been added to your cart.",
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot
                };
                _ = successDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AddToCart: {ex.Message}");

                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Failed to add item to cart. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot
                };
                _ = errorDialog.ShowAsync();
            }
        }

        private void CardBorder_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 255, 153, 51));
                border.BorderThickness = new Thickness(2);

                if (border.RenderTransform is ScaleTransform scale)
                {
                    scale.ScaleX = 1.02;
                    scale.ScaleY = 1.02;
                }
            }
        }

        private void CardBorder_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 255, 229, 204));
                border.BorderThickness = new Thickness(1);

                if (border.RenderTransform is ScaleTransform scale)
                {
                    scale.ScaleX = 1.0;
                    scale.ScaleY = 1.0;
                }
            }
        }
    }
}