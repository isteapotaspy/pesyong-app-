using PESYONG.ApplicationLogic.Services;
using PESYONG.Domain.Entities;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Meals.MealProduct;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PESYONG.Presentation.ViewModels
{
    /// <summary>
    /// Manages the presentation logic for individual Kakanin products.
    /// Handles unit displays, stock validation, and synchronization with the cart.
    /// </summary>
    public class KakaninViewModel : INotifyPropertyChanged
    {
        private readonly Meal _meal;
        private readonly CartService _cartService;
        private int _selectedQuantity;
        private int _cartQuantity;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int MealProductID => _meal.MealID ?? 0;
        public string MealName => _meal.MealName;
        public string? Description => _meal.Description;
        public decimal MealPrice => _meal.MealPrice;
        public byte[]? ImageBytes => _meal.ImageBytes;
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

            UpdateCartQuantity();

            if (_cartService.Cart != null)
            {
                _cartService.Cart.CollectionChanged += (s, e) => UpdateCartQuantity();
            }
        }

        private void UpdateCartQuantity()
        {
            if (_cartService?.Cart == null)
            {
                CartQuantity = 0;
                return;
            }

            CartQuantity = _cartService.Cart
                .Where(x => x.ProductId == MealProductID && x.Type == "kakanin")
                .Sum(x => x.Quantity);
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
            var existingItem = _cartService.Cart
                .FirstOrDefault(x => x.ProductId == MealProductID && x.Type == "kakanin");

            if (existingItem != null)
            {
                _cartService.UpdateQuantity(existingItem.Id, existingItem.Quantity + SelectedQuantity);
            }
            else
            {
                var cartItem = new CartItem
                {
                    Id = $"kakanin_{MealProductID}",
                    Name = MealName,
                    Price = (double)MealPrice,
                    Quantity = SelectedQuantity,
                    ImageBytes = ImageBytes,
                    Type = "kakanin",
                    ProductId = MealProductID
                };

                _cartService.AddToCart(cartItem);
            }

            UpdateCartQuantity();
            SelectedQuantity = MinOrderQuantity;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}