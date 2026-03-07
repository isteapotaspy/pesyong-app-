using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using PESYONG.ApplicationLogic.Services;
using PESYONG.Domain.Entities;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Enums;
using PESYONG.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PESYONG.Presentation.Views.Customer
{
    /// <summary>
    /// Represents the customer-facing storefront for Kakanin products.
    /// Handles the initialization of product data, hover animations for product cards, 
    /// and coordinates cart interactions between the UI and the <see cref="CartService"/>.
    /// </summary>
    public sealed partial class KakaninPage : Page
    {
        private ObservableCollection<KakaninViewModel> KakaninItems { get; set; }
        private readonly CartService _cartService;

        public KakaninPage()
        {
            this.InitializeComponent();
            _cartService = CartService.Instance; // get this from DI
            this.Loaded += async (_, _) => LoadKakaninAsync();
        }

        private async Task LoadKakaninAsync()
        {
            try
            {
                var meals = new List<Meal>
        {
            new Meal
            {
                MealID = 1,
                MealName = "Puto",
                MealPrice = 60,
                Description = "Soft and fluffy steamed rice cake",
                ImageBytes = await LoadImageBytesAsync("Assets/Images/puto.jpg"),
                StockQuantity = 50,
                MinOrderQuantity = 6,
                MealTags = new List<string> { "Makakalibanga", "Makapapurigit" }
            },
            new Meal
            {
                MealID = 2,
                MealName = "Kutsinta",
                MealPrice = 50,
                Description = "Brown rice cake with coconut topping",
                ImageBytes = await LoadImageBytesAsync("Assets/Images/kutsinta.jpg"),
                StockQuantity = 45,
                MinOrderQuantity = 6,
                MealTags = new List<string> { "Makakalibanga", "Makapapurigit" }
            },
            new Meal
            {
                MealID = 3,
                MealName = "Bibingka",
                MealPrice = 80,
                Description = "Traditional baked rice cake",
                ImageBytes = await LoadImageBytesAsync("Assets/Images/bibingka.jpg"),
                StockQuantity = 30,
                MinOrderQuantity = 1,
                MealTags = new List<string> { "Makakalibanga", "Makapapurigit" }
            },
            new Meal
            {
                MealID = 4,
                MealName = "Suman",
                MealPrice = 70,
                Description = "Sticky rice wrapped in banana leaves",
                ImageBytes = await LoadImageBytesAsync("Assets/Images/suman.jpg"),
                StockQuantity = 40,
                MinOrderQuantity = 6,
                MealTags = new List<string> { "Makakalibanga", "Makapapurigit" }
            },
            new Meal
            {
                MealID = 5,
                MealName = "Sapin-Sapin",
                MealPrice = 90,
                Description = "Multi-layered sweet rice cake",
                ImageBytes = await LoadImageBytesAsync("Assets/Images/sapin-sapin.jpg"),
                StockQuantity = 25,
                MinOrderQuantity = 1,
                MealTags = new List<string> { "Makakalibanga", "Makapapurigit" }
            },
            new Meal
            {
                MealID = 6,
                MealName = "Biko",
                MealPrice = 75,
                Description = "Sweet sticky rice with coconut caramel",
                ImageBytes = await LoadImageBytesAsync("Assets/Images/biko.jpg"),
                StockQuantity = 35,
                MinOrderQuantity = 1,
                MealTags = new List<string> { "Makakalibanga", "Makapapurigit" }
            }
        };

                var viewModels = meals.Select(m => new KakaninViewModel(m, _cartService));
                KakaninItems = new ObservableCollection<KakaninViewModel>(viewModels);
                KakaninItemsControl.ItemsSource = KakaninItems;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadKakaninAsync: {ex.Message}");
            }
        }

        private async Task<byte[]?> LoadImageBytesAsync(string relativePath)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(
                    new Uri($"ms-appx:///{relativePath}"));

                using IRandomAccessStream stream = await file.OpenReadAsync();
                byte[] bytes = new byte[stream.Size];
                await stream.ReadAsync(bytes.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);

                return bytes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load image bytes: {ex.Message}");
                return null;
            }
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

}