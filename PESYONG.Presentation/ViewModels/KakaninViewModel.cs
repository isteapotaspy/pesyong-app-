using Microsoft.UI.Xaml.Controls;
using PESYONG.ApplicationLogic.Services;
using PESYONG.Domain.Entities;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Presentation.Views.Customer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.Presentation.ViewModels
{
    // ViewModel for Kakanin items
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
                // Create cart item
                var cartItem = new CartItem
                {
                    Id = $"kakanin_{MealID}",
                    Name = MealName,
                    Price = (double)MealPrice,
                    Quantity = SelectedQuantity,
                    Image = ImageSourceString,
                    Type = "kakanin",
                    ProductId = MealID
                };

                _cartService.AddToCart(cartItem);

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
