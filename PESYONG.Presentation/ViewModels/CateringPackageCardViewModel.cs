using Microsoft.UI.Xaml.Media.Imaging;
using PESYONG.Domain.Entities.Meals.MealProduct;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;

namespace PESYONG.Presentation.ViewModels
{
    public class CateringPackageCardViewModel : INotifyPropertyChanged
    {
        private int _cartQuantity;
        private BitmapImage? _imageSource;
        private byte[]? _imageBytes;

        public MealProduct Package { get; }
        public int MealProductID => Package.MealProductID;
        public string ProductName => Package.ProductName;
        public string ProductDescription => Package.ProductDescription;
        public decimal ProductBasePrice => Package.ProductBasePrice;
        public int PaxCount => Package.PaxCount;
        public string PaxDisplay => Package.PaxDisplay;
        public ICollection<MealProductItem> MealProductItems => Package.MealProductItems;
        public int PreferredViandCount => Package.PreferredViandCount;
        public bool IsCustomizable => Package.IsCustomizable;

        public byte[]? ImageBytes => _imageBytes;

        public BitmapImage? ImageSource
        {
            get => _imageSource;
            private set
            {
                _imageSource = value;
                OnPropertyChanged();
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

        public string ViandSelectionText =>
            IsCustomizable && PreferredViandCount > 0
        ? $"🔸 Choose any {PreferredViandCount} viands from our menu"
        : string.Empty;


        public CateringPackageCardViewModel(MealProduct package)
        {
            Package = package;
            _ = LoadImageAsync();
        }

        private async System.Threading.Tasks.Task LoadImageAsync()
        {
            try
            {
                _imageBytes = Package.ImageBytes
                    ?? Package.MealProductItems?.FirstOrDefault()?.Meal?.ImageBytes;

                if (_imageBytes == null || _imageBytes.Length == 0)
                {
                    ImageSource = null;
                    return;
                }

                using var memoryStream = new MemoryStream(_imageBytes);
                var bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(memoryStream.AsRandomAccessStream());
                ImageSource = bitmap;

                OnPropertyChanged(nameof(ImageBytes));
            }
            catch
            {
                _imageBytes = null;
                ImageSource = null;
                OnPropertyChanged(nameof(ImageBytes));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}