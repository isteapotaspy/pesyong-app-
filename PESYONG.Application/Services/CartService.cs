using PESYONG.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PESYONG.ApplicationLogic.Services
{
    /// <summary>
    /// Provides cart management operations for the customer ordering flow,
    /// including item handling, delivery details, and total calculations.
    /// </summary>
    public class CartService
    {
        private static CartService _instance;

        /// <summary>
        /// Gets the singleton instance of the cart service.
        /// </summary>
        public static CartService Instance => _instance ??= new CartService();

        /// <summary>
        /// Gets the collection of items currently in the cart.
        /// </summary>
        public ObservableCollection<CartItem> Cart { get; } = new ObservableCollection<CartItem>();

        /// <summary>
        /// Gets or sets the delivery information associated with the cart.
        /// </summary>
        public DeliveryInfo? Delivery { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CartService"/> class.
        /// </summary>
        private CartService()
        {
            Delivery = new DeliveryInfo();
        }

        /// <summary>
        /// Adds an item to the cart. If the same item already exists,
        /// its quantity is increased instead.
        /// </summary>
        /// <param name="item">The cart item to add.</param>
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

        /// <summary>
        /// Removes an item from the cart by its ID.
        /// </summary>
        /// <param name="itemId">The ID of the item to remove.</param>
        public void RemoveFromCart(string itemId)
        {
            var item = Cart.FirstOrDefault(x => x.Id == itemId);
            if (item != null)
            {
                Cart.Remove(item);
                CartUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Updates the quantity of a cart item. If the new quantity is zero
        /// or less, the item is removed from the cart.
        /// </summary>
        /// <param name="itemId">The ID of the item to update.</param>
        /// <param name="newQuantity">The new quantity value.</param>
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

        /// <summary>
        /// Clears all cart items and resets delivery information.
        /// </summary>
        public void ClearCart()
        {
            Cart.Clear();
            Delivery = null;
            CartUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Calculates the total quantity of all items in the cart.
        /// </summary>
        /// <returns>The total number of items in the cart.</returns>
        public int GetTotalItemCount()
        {
            return Cart.Sum(item => item.Quantity);
        }

        /// <summary>
        /// Calculates the subtotal of all cart items before delivery fees.
        /// </summary>
        /// <returns>The cart subtotal.</returns>
        public double GetSubtotal()
        {
            return Cart.Sum(item => item.Price * item.Quantity);
        }

        /// <summary>
        /// Calculates the final total including delivery fees.
        /// </summary>
        /// <returns>The overall total amount.</returns>
        public double GetTotal()
        {
            return GetSubtotal() + (Delivery?.DeliveryFee ?? 0);
        }

        /// <summary>
        /// Sets the delivery information for the current cart.
        /// </summary>
        /// <param name="deliveryInfo">The delivery information to assign.</param>
        public void SetDelivery(DeliveryInfo deliveryInfo)
        {
            Delivery = deliveryInfo;
        }

        /// <summary>
        /// Occurs when the cart contents or delivery information change.
        /// </summary>
        public event EventHandler? CartUpdated;
    }
}