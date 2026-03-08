using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PESYONG.ApplicationLogic.Services
{
    public class CartStateService : INotifyPropertyChanged
    {
        private readonly Dictionary<string, int> _itemQuantities = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? CartChanged;

        public int TotalCartCount => _itemQuantities.Values.Sum();

        public int GetItemQuantity(string key)
        {
            return _itemQuantities.TryGetValue(key, out int quantity) ? quantity : 0;
        }

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

        public void RemoveItem(string key)
        {
            if (_itemQuantities.Remove(key))
            {
                NotifyCartChanged();
            }
        }

        public void Clear()
        {
            _itemQuantities.Clear();
            NotifyCartChanged();
        }

        private void NotifyCartChanged()
        {
            OnPropertyChanged(nameof(TotalCartCount));
            CartChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}