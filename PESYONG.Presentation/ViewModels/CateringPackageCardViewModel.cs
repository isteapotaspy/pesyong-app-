using Microsoft.Identity.Client;
using PESYONG.Domain.Entities.Meals.MealProduct;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.Presentation.ViewModels
{
    public class CateringPackageCardViewModel : INotifyPropertyChanged
    {
        private int _cartQuantity;

        public MealProduct Package { get; }
        public int MealProductID => Package.MealProductID;
        public string ProductName => Package.ProductName;
        public string ProductDescription => Package.ProductDescription;
        public decimal ProductBasePrice => Package.ProductBasePrice;
        public int PaxCount => Package.PaxCount;
        public string PaxDisplay => Package.PaxDisplay;
        public ICollection<MealProductItem> MealProductItems => Package.MealProductItems;
        public byte[]? ImageBytes => Package.MealProductItems?
                .FirstOrDefault()?.Meal?.ImageBytes;

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

        public CateringPackageCardViewModel(MealProduct package)
        {
            Package = package;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
