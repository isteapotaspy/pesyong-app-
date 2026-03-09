using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PESYONG.ApplicationLogic.Services
{
    /// <summary>
    /// Maintains cart quantity state for UI binding and notifies listeners
    /// when cart item counts change.
    /// </summary>
    public class CartStateService : INotifyPropertyChanged
    {
        private readonly Dictionary<string, int> _itemQuantities = new();

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Occurs when the cart state changes.
        /// </summary>
        public event EventHandler? CartChanged;

        /// <summary>
        /// Gets the total quantity of all items currently tracked in the cart.
        /// </summary>
        public int TotalCartCount => _itemQuantities.Values.Sum();

        /// <summary>
        /// Gets the quantity of a specific cart item by key.
        /// </summary>
        /// <param name="key">The unique key of the cart item.</param>
        /// <returns>The tracked quantity of the item, or 0 if not found.</returns>
        public int GetItemQuantity(string key)
        {
            return _itemQuantities.TryGetValue(key, out int quantity) ? quantity : 0;
        }

        /// <summary>
        /// Adds a new cart item or increases the quantity of an existing one.
        /// </summary>
        /// <param name="key">The unique key of the cart item.</param>
        /// <param name="quantity">The quantity to add.</param>
        public void AddOrIncreaseItem(string key, int quantity)
        {
            if (string.IsNullOrWhiteSpace(key) || quantity <= 0)
                return;

            if (_itemQuantities.ContainsKey(key))
                _itemQuantities[key] += quantity;
            else
                _itemQuantities[key] = quantity;

            NotifyCartChanged();
        }

        /// <summary>
        /// Sets the quantity of a cart item directly.
        /// Removes the item if the quantity is zero or less.
        /// </summary>
        /// <param name="key">The unique key of the cart item.</param>
        /// <param name="quantity">The quantity to assign.</param>
        public void SetItemQuantity(string key, int quantity)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            if (quantity <= 0)
                _itemQuantities.Remove(key);
            else
                _itemQuantities[key] = quantity;

            NotifyCartChanged();
        }

        /// <summary>
        /// Removes a cart item from the tracked state.
        /// </summary>
        /// <param name="key">The unique key of the cart item.</param>
        public void RemoveItem(string key)
        {
            if (_itemQuantities.Remove(key))
            {
                NotifyCartChanged();
            }
        }

        /// <summary>
        /// Clears all tracked cart item quantities.
        /// </summary>
        public void Clear()
        {
            _itemQuantities.Clear();
            NotifyCartChanged();
        }

        /// <summary>
        /// Raises cart-related change notifications.
        /// </summary>
        private void NotifyCartChanged()
        {
            OnPropertyChanged(nameof(TotalCartCount));
            CartChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}