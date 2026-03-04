using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using PESYONG.ApplicationLogic.Services;
using PESYONG.Domain.Entities;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PESYONG.Presentation.Views.Customer
{
    public sealed partial class KakaninPage : Page
    {
        private ObservableCollection<KakaninViewModel> KakaninItems { get; set; }
        private readonly CartService _cartService;

        public KakaninPage()
        {
            this.InitializeComponent();
            _cartService = CartService.Instance; // get this from DI
            LoadKakanin();
        }

        private void LoadKakanin()
        {
            //this would come from your database via a service
            // Filtering meals that are tagged as "Kakanin" or have specific MealTagType
            var meals = new List<Meal>
            {
                new Meal
                {
                    MealID = 1,
                    MealName = "Puto",
                    MealPrice = 60,
                    Description = "Soft and fluffy steamed rice cake",
                    ImageSourceString = "ms-appx:///Assets/Images/puto.jpg",
                    StockQuantity = 50,
                    MinOrderQuantity = 6, // Sold by dozens
                    MealTags = new List<MealTagType> { MealTagType.Kakanin, MealTagType.Dessert }
                },
                new Meal
                {
                    MealID = 2,
                    MealName = "Kutsinta",
                    MealPrice = 50,
                    Description = "Brown rice cake with coconut topping",
                    ImageSourceString = "ms-appx:///Assets/Images/kutsinta.jpg",
                    StockQuantity = 45,
                    MinOrderQuantity = 6,
                    MealTags = new List<MealTagType> { MealTagType.Kakanin, MealTagType.Dessert }
                },
                new Meal
                {
                    MealID = 3,
                    MealName = "Bibingka",
                    MealPrice = 80,
                    Description = "Traditional baked rice cake",
                    ImageSourceString = "ms-appx:///Assets/Images/bibingka.jpg",
                    StockQuantity = 30,
                    MinOrderQuantity = 1,
                    MealTags = new List<MealTagType> { MealTagType.Kakanin, MealTagType.Dessert }
                },
                new Meal
                {
                    MealID = 4,
                    MealName = "Suman",
                    MealPrice = 70,
                    Description = "Sticky rice wrapped in banana leaves",
                    ImageSourceString = "ms-appx:///Assets/Images/suman.jpg",
                    StockQuantity = 40,
                    MinOrderQuantity = 6,
                    MealTags = new List<MealTagType> { MealTagType.Kakanin, MealTagType.Dessert }
                },
                new Meal
                {
                    MealID = 5,
                    MealName = "Sapin-Sapin",
                    MealPrice = 90,
                    Description = "Multi-layered sweet rice cake",
                    ImageSourceString = "ms-appx:///Assets/Images/sapin-sapin.jpg",
                    StockQuantity = 25,
                    MinOrderQuantity = 1,
                    MealTags = new List<MealTagType> { MealTagType.Kakanin, MealTagType.Dessert }
                },
                new Meal
                {
                    MealID = 6,
                    MealName = "Biko",
                    MealPrice = 75,
                    Description = "Sweet sticky rice with coconut caramel",
                    ImageSourceString = "ms-appx:///Assets/Images/biko.jpg",
                    StockQuantity = 35,
                    MinOrderQuantity = 1,
                    MealTags = new List<MealTagType> { MealTagType.Kakanin, MealTagType.Dessert }
                }
            };

            // Convert to ViewModels
            var viewModels = meals.Select(m => new KakaninViewModel(m, _cartService));
            KakaninItems = new ObservableCollection<KakaninViewModel>(viewModels);
            KakaninItemsControl.ItemsSource = KakaninItems;
        }

        private void CardBorder_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                // Scale up slightly on hover
                border.RenderTransform = new ScaleTransform { ScaleX = 1.02, ScaleY = 1.02 };
            }
        }

        private void CardBorder_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                // Reset to original size
                border.RenderTransform = new ScaleTransform { ScaleX = 1.0, ScaleY = 1.0 };
            }
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int mealId)
            {
                var item = KakaninItems.FirstOrDefault(x => x.MealID == mealId);
                item?.IncreaseQuantity();
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int mealId)
            {
                var item = KakaninItems.FirstOrDefault(x => x.MealID == mealId);
                item?.DecreaseQuantity();
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int mealId)
            {
                var item = KakaninItems.FirstOrDefault(x => x.MealID == mealId);
                item?.AddToCart();

                // Show success message
                var dialog = new ContentDialog
                {
                    Title = "Added to Cart",
                    Content = $"{item?.SelectedQuantity} {item?.MealName} added to your cart!",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = dialog.ShowAsync();
            }
        }
    }

    // ViewModel for Kakanin items with INotifyPropertyChanged
    public class KakaninViewModel : INotifyPropertyChanged
    {
        private readonly Meal _meal;
        private readonly CartService _cartService;
        private int _selectedQuantity;
        private int _cartQuantity;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int MealID => _meal.MealID;
        public string MealName => _meal.MealName;
        public string? Description => _meal.Description;
        public decimal MealPrice => _meal.MealPrice;
        public string ImageSourceString => _meal.ImageSourceString;
        public int StockQuantity => _meal.StockQuantity;
        public int MinOrderQuantity => _meal.MinOrderQuantity;

        public string UnitDisplay => _meal.MinOrderQuantity >= 6 ? "per dozen" : "per piece";

        public bool IsAvailable => StockQuantity > 0;

        public string AvailabilityStatus => IsAvailable ? "In Stock" : "Out of Stock";

        public int SelectedQuantity
        {
            get => _selectedQuantity;
            set
            {
                if (_selectedQuantity != value)
                {
                    _selectedQuantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalPrice));
                    OnPropertyChanged(nameof(CanIncreaseQuantity));
                    OnPropertyChanged(nameof(CanDecreaseQuantity));
                }
            }
        }

        public int CartQuantity
        {
            get => _cartQuantity;
            set
            {
                if (_cartQuantity != value)
                {
                    _cartQuantity = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal TotalPrice => MealPrice * SelectedQuantity;

        public bool CanIncreaseQuantity => SelectedQuantity < 20 && SelectedQuantity < StockQuantity;
        public bool CanDecreaseQuantity => SelectedQuantity > MinOrderQuantity;

        public KakaninViewModel(Meal meal, CartService cartService)
        {
            _meal = meal;
            _cartService = cartService;
            _selectedQuantity = meal.MinOrderQuantity;

            // Initialize cart quantity from cart service
            UpdateCartQuantity();

            // Subscribe to cart collection changes
            _cartService.Cart.CollectionChanged += (s, e) => UpdateCartQuantity();
        }

        private void UpdateCartQuantity()
        {
            var cartItem = _cartService.Cart.FirstOrDefault(x => x.Id == MealID.ToString() && x.Type == "kakanin");
            CartQuantity = cartItem?.Quantity ?? 0;
        }

        public void IncreaseQuantity()
        {
            if (CanIncreaseQuantity)
            {
                SelectedQuantity++;
            }
        }

        public void DecreaseQuantity()
        {
            if (CanDecreaseQuantity)
            {
                SelectedQuantity--;
            }
        }

        public void AddToCart()
        {
            // Check if item already exists in cart
            var existingItem = _cartService.Cart.FirstOrDefault(x => x.Id == MealID.ToString() && x.Type == "kakanin");

            if (existingItem != null)
            {
                // Update quantity using CartService's UpdateQuantity method
                _cartService.UpdateQuantity(existingItem.Id, existingItem.Quantity + SelectedQuantity);
            }
            else
            {
                // Create new cart item using your CartItem entity
                var cartItem = new CartItem
                {
                    Id = MealID.ToString(),
                    Name = MealName,
                    Price = (double)MealPrice,
                    Quantity = SelectedQuantity,
                    Image = ImageSourceString,
                    Type = "kakanin",
                    ProductId = MealID
                };

                // Add to cart using the service's Cart ObservableCollection
                _cartService.Cart.Add(cartItem);
            }

            // Update cart quantity display
            UpdateCartQuantity();

            // Reset selected quantity to minimum
            SelectedQuantity = MinOrderQuantity;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}