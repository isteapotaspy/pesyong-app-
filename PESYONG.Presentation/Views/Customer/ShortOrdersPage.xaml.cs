using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.ApplicationLogic.Services;
using PESYONG.Domain.Entities;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Presentation.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace PESYONG.Presentation.Views.Customer
{
    /// <summary>
    /// Page for displaying and ordering individual viands/short orders.
    /// Users can adjust quantities and add items to cart.
    /// </summary>
    public sealed partial class ShortOrdersPage : Page
    {
        private readonly MealRepository _mealRepository;
        private readonly MealSyncService _mealSyncService;
        private readonly CartService _cartService;

        private ObservableCollection<ShortOrderViewModel> ShortOrders { get; set; } = new();

        public ShortOrdersPage()
        {
            this.InitializeComponent();

            _mealRepository = App.Current.Services.GetRequiredService<MealRepository>();
            _mealSyncService = App.Current.Services.GetRequiredService<MealSyncService>();
            _cartService = CartService.Instance;

            this.Loaded += ShortOrdersPage_Loaded;
            this.Unloaded += Page_Unloaded;

            if (_cartService.Cart != null)
            {
                _cartService.Cart.CollectionChanged += Cart_CollectionChanged;
            }

            _mealSyncService.MealsChanged += OnMealsChanged;
        }

        private async void ShortOrdersPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadShortOrdersAsync();
            UpdateCartQuantities();
        }

        /// <summary>
        /// Loads meals from the real SQL-backed repository.
        /// </summary>
        private async Task LoadShortOrdersAsync()
        {
            try
            {
                var meals = await _mealRepository.GetAllMealsAsync();

                // if you want only short orders, add filters here later
                // for now this loads all meals
                var availableMeals = meals
                    .Where(m => m.MealID.HasValue)
                    .OrderBy(m => m.MealName)
                    .ToList();

                foreach (var existing in ShortOrders)
                {
                    existing.PropertyChanged -= ShortOrder_PropertyChanged;
                }

                ShortOrders.Clear();

                foreach (var meal in availableMeals)
                {
                    var vm = new ShortOrderViewModel(
                        meal,
                        GetCartQuantityForMeal(meal.MealID!.Value)
                    );

                    vm.PropertyChanged += ShortOrder_PropertyChanged;
                    ShortOrders.Add(vm);
                }

                ShortOrdersItemsControl.ItemsSource = ShortOrders;

                UpdateCartQuantities();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load short orders: {ex.Message}");
            }
        }

        private async void OnMealsChanged()
        {
            await LoadShortOrdersAsync();
        }

        private void Cart_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateCartQuantities();
        }

        private void ShortOrder_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShortOrderViewModel.SelectedQuantity))
            {
                // optional extra UI behavior later
            }
        }

        /// <summary>
        /// Gets the current cart quantity for a specific meal.
        /// </summary>
        private int GetCartQuantityForMeal(int mealId)
        {
            if (_cartService?.Cart == null)
                return 0;

            var cartItem = _cartService.Cart.FirstOrDefault(c => c.ProductId == mealId);
            return cartItem?.Quantity ?? 0;
        }

        /// <summary>
        /// Updates the cart quantity badges for all items.
        /// </summary>
        private void UpdateCartQuantities()
        {
            if (ShortOrders == null || _cartService?.Cart == null)
                return;

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
            if (sender is not Button button || button.Tag == null)
                return;

            int mealId = Convert.ToInt32(button.Tag);
            var item = ShortOrders.FirstOrDefault(x => x.MealID == mealId);

            if (item != null)
            {
                item.DecrementQuantity();
            }
        }

        /// <summary>
        /// Handles increase quantity button click.
        /// </summary>
        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag == null)
                return;

            int mealId = Convert.ToInt32(button.Tag);
            var item = ShortOrders.FirstOrDefault(x => x.MealID == mealId);

            if (item != null)
            {
                item.IncrementQuantity();
            }
        }

        /// <summary>
        /// Handles add to cart button click.
        /// </summary>
        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag == null)
                return;

            int mealId = Convert.ToInt32(button.Tag);
            var item = ShortOrders.FirstOrDefault(x => x.MealID == mealId);

            if (item != null && item.IsAvailable)
            {
                int quantity = item.SelectedQuantity;

                var cartItem = new CartItem
                {
                    Id = $"shortorder_{mealId}",
                    Name = item.MealName,
                    Price = (double)item.MealPrice,
                    Quantity = quantity,
                    Type = "shortorder",
                    ProductId = mealId,
                    ImageBytes = item.ImageBytes
                };

                _cartService.AddToCart(cartItem);

                ShowSuccessDialog($"{quantity} {item.MealName} added to cart!");

                item.SelectedQuantity = 1;
                item.CartQuantity = GetCartQuantityForMeal(item.MealID);
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
        /// Handles page unloading to clean up event handlers.
        /// </summary>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _mealSyncService.MealsChanged -= OnMealsChanged;

            if (_cartService.Cart != null)
            {
                _cartService.Cart.CollectionChanged -= Cart_CollectionChanged;
            }

            if (ShortOrders != null)
            {
                foreach (var item in ShortOrders)
                {
                    item.PropertyChanged -= ShortOrder_PropertyChanged;
                }
            }
        }

        private void CardBorder_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                var st = border.RenderTransform as ScaleTransform;
                if (st != null)
                {
                    AnimateScale(st, 1.04);
                }
                else
                {
                    border.RenderTransform = new ScaleTransform { ScaleX = 1.04, ScaleY = 1.04 };
                }
            }
        }

        private void CardBorder_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                var st = border.RenderTransform as ScaleTransform;
                if (st != null)
                {
                    AnimateScale(st, 1.0);
                }
                else
                {
                    border.RenderTransform = new ScaleTransform { ScaleX = 1.0, ScaleY = 1.0 };
                }
            }
        }

        private void AnimateScale(ScaleTransform st, double target)
        {
            DoubleAnimation animX = new DoubleAnimation
            {
                To = target,
                Duration = new Duration(TimeSpan.FromMilliseconds(150))
            };

            DoubleAnimation animY = new DoubleAnimation
            {
                To = target,
                Duration = new Duration(TimeSpan.FromMilliseconds(150))
            };

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