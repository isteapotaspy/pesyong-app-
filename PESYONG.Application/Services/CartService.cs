using PESYONG.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PESYONG.ApplicationLogic.Services
{
    public class CartService
    {
        private static CartService _instance;
        public static CartService Instance => _instance ??= new CartService();

        public ObservableCollection<CartItem> Cart { get; } = new ObservableCollection<CartItem>();
        public DeliveryInfo? Delivery { get; set; }

        private CartService()
        {
            Delivery = new DeliveryInfo();
        }

        public void AddToCart(CartItem item)
        {
            var existingItem = Cart.FirstOrDefault(x => x.Id == item.Id && x.Type == item.Type);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                Cart.Add(item);
            }

            CartUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveFromCart(string itemId)
        {
            var item = Cart.FirstOrDefault(x => x.Id == itemId);
            if (item != null)
            {
                Cart.Remove(item);
                CartUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public void UpdateQuantity(string itemId, int newQuantity)
        {
            var item = Cart.FirstOrDefault(x => x.Id == itemId);
            if (item != null)
            {
                if (newQuantity <= 0)
                {
                    Cart.Remove(item);
                }
                else
                {
                    item.Quantity = newQuantity;
                }

                CartUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ClearCart()
        {
            Cart.Clear();
            Delivery = null;
            CartUpdated?.Invoke(this, EventArgs.Empty);
        }

        public double GetSubtotal()
        {
            return Cart.Sum(item => item.Price * item.Quantity);
        }

        public double GetTotal()
        {
            return GetSubtotal() + (Delivery?.DeliveryFee ?? 0);
        }

        public void SetDelivery(DeliveryInfo deliveryInfo)
        {
            Delivery = deliveryInfo;
        }

        public event EventHandler? CartUpdated;
    }
}