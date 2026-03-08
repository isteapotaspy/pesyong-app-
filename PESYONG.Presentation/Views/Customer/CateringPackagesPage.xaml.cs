using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private readonly CartService _cartService;

        public CateringPackagesPage()
        {
            InitializeComponent();

            _cartService = CartService.Instance;

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
                var sampleImageBytes = await LoadImageBytesAsync("Assets/SampleMeal.png");

                var packageModels = new List<MealProduct>
                {
                    new MealProduct
                    {
                        MealProductID = 1,
                        ProductName = "Package 1 - 3 Viands",
                        ProductDescription = "Perfect for small gatherings and family meals",
                        MealProductItems = new List<MealProductItem>
                        {
                            new MealProductItem
                            {
                                Meal = new Meal
                                {
                                    MealID = 1,
                                    MealName = "Battered Chicken",
                                    MealPrice = 450,
                                    ImageBytes = sampleImageBytes
                                },
                                Quantity = 1
                            },
                            new MealProductItem
                            {
                                Meal = new Meal
                                {
                                    MealID = 2,
                                    MealName = "Bihon Guisado",
                                    MealPrice = 350,
                                    ImageBytes = sampleImageBytes
                                },
                                Quantity = 1
                            },
                            new MealProductItem
                            {
                                Meal = new Meal
                                {
                                    MealID = 3,
                                    MealName = "Fish Fillet",
                                    MealPrice = 400,
                                    ImageBytes = sampleImageBytes
                                },
                                Quantity = 1
                            }
                        }
                    },
                    new MealProduct
                    {
                        MealProductID = 2,
                        ProductName = "Package 2 - 5 Viands",
                        ProductDescription = "Great for medium-sized celebrations",
                        MealProductItems = new List<MealProductItem>
                        {
                            new MealProductItem
                            {
                                Meal = new Meal
                                {
                                    MealID = 1,
                                    MealName = "Battered Chicken",
                                    MealPrice = 450,
                                    ImageBytes = sampleImageBytes
                                },
                                Quantity = 1
                            },
                            new MealProductItem
                            {
                                Meal = new Meal
                                {
                                    MealID = 4,
                                    MealName = "Buttered Shrimp",
                                    MealPrice = 550,
                                    ImageBytes = sampleImageBytes
                                },
                                Quantity = 1
                            },
                            new MealProductItem
                            {
                                Meal = new Meal
                                {
                                    MealID = 2,
                                    MealName = "Bihon Guisado",
                                    MealPrice = 350,
                                    ImageBytes = sampleImageBytes
                                },
                                Quantity = 1
                            },
                            new MealProductItem
                            {
                                Meal = new Meal
                                {
                                    MealID = 5,
                                    MealName = "Tuna Kinilaw",
                                    MealPrice = 400,
                                    ImageBytes = sampleImageBytes
                                },
                                Quantity = 1
                            },
                            new MealProductItem
                            {
                                Meal = new Meal
                                {
                                    MealID = 3,
                                    MealName = "Fish Fillet",
                                    MealPrice = 400,
                                    ImageBytes = sampleImageBytes
                                },
                                Quantity = 1
                            }
                        }
                    },
                    new MealProduct
                    {
                        MealProductID = 3,
                        ProductName = "Package 3 - 8 Viands + Free Dessert",
                        ProductDescription = "Our most popular package! Choose your favorite viands",
                        MealProductItems = new List<MealProductItem>()
                    }
                };

                Packages.Clear();

                foreach (var package in packageModels)
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
                System.Diagnostics.Debug.WriteLine($"Error in LoadPackagesAsync: {ex.Message}");
            }
        }

        private async Task<byte[]?> LoadImageBytesAsync(string relativePath)
        {
            try
            {
                var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(
                    new Uri($"ms-appx:///{relativePath}"));

                using var stream = await file.OpenReadAsync();
                byte[] bytes = new byte[stream.Size];
                await stream.ReadAsync(bytes.AsBuffer(), (uint)stream.Size, Windows.Storage.Streams.InputStreamOptions.None);

                return bytes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load image bytes: {ex.Message}");
                return null;
            }
        }

        private async Task LoadAvailableViandsAsync()
        {
            try
            {
                var sampleImageBytes = await LoadImageBytesAsync("Assets/SampleMeal.png");

                AvailableViands = new ObservableCollection<Meal>
                {
                    new Meal { MealID = 1, MealName = "Battered Chicken", MealPrice = 450, ImageBytes = sampleImageBytes },
                    new Meal { MealID = 4, MealName = "Buttered Shrimp", MealPrice = 550, ImageBytes = sampleImageBytes },
                    new Meal { MealID = 2, MealName = "Bihon Guisado", MealPrice = 350, ImageBytes = sampleImageBytes },
                    new Meal { MealID = 5, MealName = "Tuna Kinilaw", MealPrice = 400, ImageBytes = sampleImageBytes },
                    new Meal { MealID = 3, MealName = "Fish Fillet", MealPrice = 400, ImageBytes = sampleImageBytes },
                    new Meal { MealID = 6, MealName = "Pork Menudo", MealPrice = 450, ImageBytes = sampleImageBytes },
                    new Meal { MealID = 7, MealName = "Chicken Adobo", MealPrice = 400, ImageBytes = sampleImageBytes },
                    new Meal { MealID = 8, MealName = "Beef Caldereta", MealPrice = 500, ImageBytes = sampleImageBytes }
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadAvailableViandsAsync: {ex.Message}");
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

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is not Button button || button.Tag == null)
                    return;

                int packageId = Convert.ToInt32(button.Tag);
                var package = Packages.FirstOrDefault(p => p.MealProductID == packageId);

                if (package == null)
                    return;

                if (package.MealProductItems == null || !package.MealProductItems.Any())
                {
                    CurrentSelectedPackage = package;
                    SelectedViands.Clear();
                    ShowViandSelectionDialog(package);
                }
                else
                {
                    AddToCart(package, null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AddToCart_Click: {ex.Message}");
            }
        }

        private async void ShowViandSelectionDialog(CateringPackageCardViewModel package)
        {
            try
            {
                ViandsGrid.ItemsSource = AvailableViands;
                DialogDescription.Text = $"Choose your favorite dishes for {package.ProductName}";
                SelectedCountText.Text = $"Selected: 0 / 8";

                SelectedViands.Clear();

                if (ViandSelectionDialog.XamlRoot == null)
                {
                    ViandSelectionDialog.XamlRoot = XamlRoot;
                }

                await ViandSelectionDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ShowViandSelectionDialog: {ex.Message}");
            }
        }

        private void ViandCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is not CheckBox checkBox || checkBox.Tag == null)
                    return;

                int mealId = Convert.ToInt32(checkBox.Tag);
                var selectedMeal = AvailableViands.FirstOrDefault(m => m.MealID == mealId);

                if (selectedMeal != null && SelectedViands.Count < 8)
                {
                    SelectedViands.Add(selectedMeal);
                    UpdateSelectedCount();
                }
                else if (SelectedViands.Count >= 8)
                {
                    checkBox.IsChecked = false;

                    var warningDialog = new ContentDialog
                    {
                        Title = "Maximum Selection",
                        Content = "You can only select up to 8 viands.",
                        CloseButtonText = "OK",
                        XamlRoot = XamlRoot
                    };
                    _ = warningDialog.ShowAsync();
                }
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
                var selectedMeal = AvailableViands.FirstOrDefault(m => m.MealID == mealId);

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
                SelectedCountText.Text = $"Selected: {SelectedViands.Count} / 8";
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
                if (SelectedViands.Count == 8 && CurrentSelectedPackage != null)
                {
                    AddToCart(CurrentSelectedPackage, SelectedViands);
                }
                else
                {
                    args.Cancel = true;

                    var errorDialog = new ContentDialog
                    {
                        Title = "Invalid Selection",
                        Content = "Please select exactly 8 viands.",
                        CloseButtonText = "OK",
                        XamlRoot = XamlRoot
                    };
                    _ = errorDialog.ShowAsync();
                }
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
                decimal totalPrice;
                string itemName;

                if (selectedViands != null && selectedViands.Any())
                {
                    totalPrice = selectedViands.Sum(v => v.MealPrice);
                    itemName = $"{package.ProductName} (Custom)";
                }
                else
                {
                    totalPrice = package.ProductBasePrice;
                    itemName = package.ProductName;
                }

                var cartItem = new CartItem
                {
                    Id = $"package_{package.MealProductID}_{Guid.NewGuid()}",
                    Name = itemName,
                    Price = (double)totalPrice,
                    Quantity = 1,
                    Type = "package",
                    ProductId = package.MealProductID,
                    Pax = package.MealProductItems?.Count ?? 0
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

        private string GetPackagePrice(MealProduct package)
        {
            return package?.ProductBasePrice.ToString("N0") ?? "0";
        }

        private string GetViandNames(MealProduct package)
        {
            if (package?.MealProductItems == null || !package.MealProductItems.Any())
                return "Choose any 8 viands";

            return string.Join(", ", package.MealProductItems.Select(i => i.Meal?.MealName));
        }

        private bool IsSelectablePackage(MealProduct package)
        {
            return package?.MealProductItems == null || !package.MealProductItems.Any();
        }

        private void CardBorder_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 255, 153, 51)); // #FF9933
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
                border.BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 255, 229, 204)); // #FFE5CC
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