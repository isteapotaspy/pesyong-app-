using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PESYONG.Domain.Entities.Meals.MealProduct;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Enums;
using PESYONG.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PESYONG.Presentation.Views.Customer
{
    /// <summary>
    /// Manages the display and interaction of a customer's past and active orders.
    /// Provides functionality for order tracking, item reordering, 
    /// and a star-based rating system for order reviews.
    /// </summary>
    public sealed partial class OrderHistoryPage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<OrderViewModel> _orders;
        private int _currentRating = 0;
        private OrderViewModel? _selectedOrderForReview;

        public event PropertyChangedEventHandler? PropertyChanged;

        public OrderHistoryPage()
        {
            this.InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            // In a real app, this would come from your OrderService
            // Mock orders using your Order entity structure
            var orders = new List<Order>
            {
                new Order
                {
                    OrderID = Guid.NewGuid(),
                    OrderDate = new DateTime(2026, 1, 23),
                    ActualDeliveryDate = new DateTime(2026, 1, 24),
                    DeliveryStatus = DeliveryStatus.OutForDelivery,
                    OrderItems = new List<OrderMealProduct>
                    {
                        new OrderMealProduct
                        {
                            MealProduct = new MealProduct { ProductName = "Package 3 - 8 Viands" },
                            MealProductOrderQty = 1,
                            ItemPrice = 3800
                        },
                        new OrderMealProduct
                        {
                            MealProduct = new MealProduct { ProductName = "Bibingka" },
                            MealProductOrderQty = 2,
                            ItemPrice = 80
                        }
                    }
                },
                new Order
                {
                    OrderID = Guid.NewGuid(),
                    OrderDate = new DateTime(2026, 1, 20),
                    ActualDeliveryDate = new DateTime(2026, 1, 21),
                    DeliveryStatus = DeliveryStatus.Delivered,
                    OrderItems = new List<OrderMealProduct>
                    {
                        new OrderMealProduct
                        {
                            MealProduct = new MealProduct { ProductName = "Battered Chicken" },
                            MealProductOrderQty = 3,
                            ItemPrice = 120
                        },
                        new OrderMealProduct
                        {
                            MealProduct = new MealProduct { ProductName = "Puto" },
                            MealProductOrderQty = 1,
                            ItemPrice = 60
                        }
                    }
                }
            };

            _orders = new ObservableCollection<OrderViewModel>(
                orders.Select(o => new OrderViewModel(o))
            );

            OrdersList.ItemsSource = _orders;
            UpdateEmptyState();
        }

        private void UpdateEmptyState()
        {
            bool hasOrders = _orders?.Any() ?? false;
            EmptyStatePanel.Visibility = hasOrders ? Visibility.Collapsed : Visibility.Visible;
            OrdersList.Visibility = hasOrders ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ReviewButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            _selectedOrderForReview = button?.Tag as OrderViewModel;

            if (_selectedOrderForReview != null)
            {
                ReviewDialogTitle.Text = $"Review: {_selectedOrderForReview.OrderItems.First().MealProduct?.ProductName}";
                ResetStarRating();
                ReviewTextBox.Text = string.Empty;
                ReviewStatusText.Text = string.Empty;
                _ = ReviewDialog.ShowAsync();
            }
        }

        private void ReorderButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var order = button?.Tag as OrderViewModel;

            if (order != null)
            {
                // In a real app, this would add items to cart
                var dialog = new ContentDialog
                {
                    Title = "Reorder",
                    Content = $"Items from order {order.OrderID} have been added to your cart.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = dialog.ShowAsync();

                // Navigate to cart
                Frame.Navigate(typeof(CartPage));
            }
        }

        private void StarRating_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is string tag && int.TryParse(tag, out int rating))
            {
                _currentRating = rating;
                UpdateStarIcons();
            }
        }

        private void ResetStarRating()
        {
            _currentRating = 0;
            UpdateStarIcons();
        }

        private void UpdateStarIcons()
        {
            var stars = new[] { Star1Icon, Star2Icon, Star3Icon, Star4Icon, Star5Icon };
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].Foreground = new SolidColorBrush(
                    i < _currentRating ?
                    Windows.UI.Color.FromArgb(255, 255, 102, 0) : // #FF6600
                    Windows.UI.Color.FromArgb(255, 255, 178, 102)   // #FFB266
                );
            }
        }

        private void ReviewDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (_currentRating == 0)
            {
                ReviewStatusText.Text = "Please select a rating";
                args.Cancel = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(ReviewTextBox.Text))
            {
                ReviewStatusText.Text = "Please enter your review";
                args.Cancel = true;
                return;
            }

            //save the review to your database
            var reviewData = new
            {
                OrderId = _selectedOrderForReview?.OrderID,
                Rating = _currentRating,
                Review = ReviewTextBox.Text,
                Date = DateTime.Now
            };

            // Show success notification
            var successDialog = new ContentDialog
            {
                Title = "Review Submitted",
                Content = "Thank you for your feedback!",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            _ = successDialog.ShowAsync();
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
    
