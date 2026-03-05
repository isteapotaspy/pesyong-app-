using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using PESYONG.ApplicationLogic.Services;
using PESYONG.Domain.Entities;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Enums;
using PESYONG.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI;

namespace PESYONG.Presentation.Views.Customer
{
    /// <summary>
    /// Page for displaying and ordering individual viands/short orders.
    /// Users can adjust quantities and add items to cart.
    /// </summary>
    public sealed partial class ShortOrdersPage : Page
    {
        private ObservableCollection<ShortOrderViewModel> ShortOrders { get; set; }
        private CartService _cartService;

        public ShortOrdersPage()
        {
            this.InitializeComponent();

            // Initialize cart service without depending on AppUser
            _cartService = CartService.Instance;

            LoadShortOrders();
            UpdateCartQuantities();

            // Subscribe to cart changes
            if (_cartService.Cart != null)
            {
                _cartService.Cart.CollectionChanged += (s, e) => UpdateCartQuantities();
            }
        }

        /// <summary>
        /// Loads the available short orders (individual viands) using Meal entities.
        /// </summary>
        private void LoadShortOrders()
        {
            // Create sample Meal entities based on your domain model
            var meals = new List<Meal>
            {
                new Meal
                {
                    MealID = 1,
                    MealName = "Battered Chicken",
                    MealPrice = 150,
                    Description = "Crispy golden fried chicken",
                    ImageSourceString = "ms-appx:///Assets/Images/battered-chicken.jpg",
                    StockQuantity = 50,
                    MinOrderQuantity = 1,
                    DeliveryType = DeliveryType.Delivery,
                    MealTags = new List<String> { "Makakalibanga", "Makapapurigit" }
                },
                new Meal
                {
                    MealID = 2,
                    MealName = "Buttered Shrimp",
                    MealPrice = 200,
                    Description = "Succulent shrimp in garlic butter sauce",
                    ImageSourceString = "ms-appx:///Assets/Images/buttered-shrimp.jpg",
                    StockQuantity = 30,
                    MinOrderQuantity = 1,
                    DeliveryType = DeliveryType.Delivery,
                    MealTags = new List<String> { "Makakalibanga", "Makapapurigit" }
                },
                new Meal
                {
                    MealID = 3,
                    MealName = "Bihon Guisado",
                    MealPrice = 120,
                    Description = "Filipino stir-fried rice noodles",
                    ImageSourceString = "ms-appx:///Assets/Images/bihon-guisado.jpg",
                    StockQuantity = 40,
                    MinOrderQuantity = 1,
                    DeliveryType = DeliveryType.Delivery,
                    MealTags = new List<String> { "Makakalibanga", "Makapapurigit" }
                },
                new Meal
                {
                    MealID = 4,
                    MealName = "Fish Fillet",
                    MealPrice = 180,
                    Description = "Perfectly seasoned fried fish fillet",
                    ImageSourceString = "ms-appx:///Assets/Images/fish-fillet.jpg",
                    StockQuantity = 25,
                    MinOrderQuantity = 1,
                    DeliveryType = DeliveryType.Delivery,
                    MealTags = new List<String> { "Makakalibanga", "Makapapurigit" }
                },
                new Meal
                {
                    MealID = 5,
                    MealName = "Tuna Kinilaw",
                    MealPrice = 220,
                    Description = "Fresh tuna ceviche Filipino-style",
                    ImageSourceString = "ms-appx:///Assets/Images/tuna-kinilaw.jpg",
                    StockQuantity = 15,
                    MinOrderQuantity = 1,
                    DeliveryType = DeliveryType.Delivery,
                    MealTags = new List<String> { "Makakalibanga", "Makapapurigit" }
                },
                new Meal
                {
                    MealID = 6,
                    MealName = "Pork Menudo",
                    MealPrice = 160,
                    Description = "Savory pork and vegetable stew",
                    ImageSourceString = "ms-appx:///Assets/Images/pork-menudo.jpg",
                    StockQuantity = 35,
                    MinOrderQuantity = 1,
                    DeliveryType = DeliveryType.Delivery,
                    MealTags = new List<String> { "Makakalibanga", "Makapapurigit" }
                }
            };

            // Convert Meal entities to ShortOrderViewModels
            ShortOrders = new ObservableCollection<ShortOrderViewModel>(
                meals.Select(meal => new ShortOrderViewModel(meal, GetCartQuantityForMeal(meal.MealID.Value)))
            );

            // Subscribe to property changes for UI updates
            foreach (var item in ShortOrders)
            {
                item.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(ShortOrderViewModel.SelectedQuantity))
                    {
                        // Quantity changed - update UI state if needed
                        // The TotalPrice is automatically updated in the ViewModel
                    }
                };
            }

            ShortOrdersItemsControl.ItemsSource = ShortOrders;
        }

        /// <summary>
        /// Gets the current cart quantity for a specific meal
        /// </summary>
        private int GetCartQuantityForMeal(int mealId)
        {
            if (_cartService?.Cart == null) return 0;
            var cartItem = _cartService.Cart.FirstOrDefault(c => c.ProductId == mealId);
            return cartItem?.Quantity ?? 0;
        }

        /// <summary>
        /// Updates the cart quantity badges for all items.
        /// </summary>
        private void UpdateCartQuantities()
        {
            if (ShortOrders == null || _cartService?.Cart == null) return;

            foreach (var item in ShortOrders)
            {
                item.CartQuantity = GetCartQuantityForMeal(item.MealID);
            }
        }

        /// <summary>
        /// Handles decrease quantity button click.
        /// </summary>
        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            int mealId = Convert.ToInt32(button.Tag);
            var item = ShortOrders.FirstOrDefault(x => x.MealID == mealId);

            if (item != null)
            {
                // Use the ViewModel's method which includes validation
                item.DecrementQuantity();
            }
        }

        /// <summary>
        /// Handles increase quantity button click.
        /// </summary>
        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            int mealId = Convert.ToInt32(button.Tag);
            var item = ShortOrders.FirstOrDefault(x => x.MealID == mealId);

            if (item != null)
            {
                // Use the ViewModel's method which includes validation
                item.IncrementQuantity();
            }
        }

        /// <summary>
        /// Handles add to cart button click.
        /// </summary>
        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            int mealId = Convert.ToInt32(button.Tag);
            var item = ShortOrders.FirstOrDefault(x => x.MealID == mealId);

            if (item != null && item.IsAvailable)
            {
                int quantity = item.SelectedQuantity;

                // Create cart item
                var cartItem = new CartItem
                {
                    Id = $"shortorder_{mealId}",
                    Name = item.MealName,
                    Price = (double)item.MealPrice,
                    Quantity = quantity,
                    Type = "shortorder",
                    ProductId = mealId
                };

                _cartService.AddToCart(cartItem);

                // Show success message
                ShowSuccessDialog($"{quantity} {item.MealName} added to cart!");

                item.SelectedQuantity = 1;

                // The ViewModel's AddToCart method already resets the quantity
                // and updates cart quantity, so we don't need to do it here
            }
            else if (item != null && !item.IsAvailable)
            {
                ShowErrorDialog($"Sorry, {item.MealName} is currently out of stock.");
            }
        }

        /// <summary>
        /// Shows a success dialog.
        /// </summary>
        private async void ShowSuccessDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Added to Cart!",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Shows an error dialog.
        /// </summary>
        private async void ShowErrorDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Unable to Add to Cart",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Handles page unloading to clean up event handlers
        /// </summary>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (ShortOrders != null)
            {
                foreach (var item in ShortOrders)
                {
                    item.PropertyChanged -= (s, e) => { }; // Remove event handlers if any were added
                }
            }
        }

        private void CardBorder_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                border.RenderTransform = new ScaleTransform { ScaleX = 1.04, ScaleY = 1.04 };
            }
        }

        private void CardBorder_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                border.RenderTransform = new ScaleTransform { ScaleX = 1.0, ScaleY = 1.0 };
            }
        }

        private void AnimateScale(ScaleTransform st, double target)
        {
            DoubleAnimation animX = new DoubleAnimation { To = target, Duration = new Duration(TimeSpan.FromMilliseconds(150)) };
            DoubleAnimation animY = new DoubleAnimation { To = target, Duration = new Duration(TimeSpan.FromMilliseconds(150)) };

            Storyboard sb = new Storyboard();
            Storyboard.SetTarget(animX, st);
            Storyboard.SetTargetProperty(animX, "ScaleX");
            Storyboard.SetTarget(animY, st);
            Storyboard.SetTargetProperty(animY, "ScaleY");

            sb.Children.Add(animX);
            sb.Children.Add(animY);
            sb.Begin();
        }
    }
}
