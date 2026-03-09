using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.Domain.Enums;
using PESYONG.Presentation.Services;
using PESYONG.Presentation.ViewModels.ObjectModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PESYONG.Presentation.Views.Customer
{
    /// <summary>
    /// Manages the display and interaction of a customer's past and active orders.
    /// Provides functionality for order tracking, item reordering,
    /// and a star-based rating system for order reviews.
    /// </summary>
    public sealed partial class OrderHistoryPage : Page, INotifyPropertyChanged
    {
        private readonly OrderRepository _orderRepository;
        private readonly RealtimeService _realtimeService;

        private ObservableCollection<OrderViewModel> _orders = new();
        private int _currentRating = 0;
        private OrderViewModel? _selectedOrderForReview;

        public event PropertyChangedEventHandler? PropertyChanged;

        public OrderHistoryPage()
        {
            InitializeComponent();

            _orderRepository = App.Current.Services.GetRequiredService<OrderRepository>();
            _realtimeService = App.Current.Services.GetRequiredService<RealtimeService>();

            Loaded += OrderHistoryPage_Loaded;
            Unloaded += OrderHistoryPage_Unloaded;
        }

        private async void OrderHistoryPage_Loaded(object sender, RoutedEventArgs e)
        {
            _realtimeService.OrderStatusChanged += OnOrderStatusChanged;
            await LoadOrdersAsync();
        }

        private void OrderHistoryPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _realtimeService.OrderStatusChanged -= OnOrderStatusChanged;
        }

        private async Task OnOrderStatusChanged(Guid orderId, string status)
        {
            System.Diagnostics.Debug.WriteLine($"OrderStatusChanged received: {orderId} -> {status}");
            await LoadOrdersAsync();
        }

        private async Task LoadOrdersAsync()
        {
            try
            {
                var customerId = App.Current.CurrentCustomerId;
                System.Diagnostics.Debug.WriteLine($"OrderHistoryPage CurrentCustomerId: {customerId}");

                if (customerId == null || customerId == Guid.Empty)
                {
                    _orders.Clear();
                    OrdersList.ItemsSource = _orders;
                    UpdateEmptyState();
                    return;
                }

                var orders = await _orderRepository.GetOrdersByCustomerAsync(customerId.Value);
                System.Diagnostics.Debug.WriteLine($"Orders loaded for customer {customerId}: {orders.Count}");

                _orders = new ObservableCollection<OrderViewModel>(
                    orders.Select(o =>
                    {
                        var vm = new OrderViewModel();
                        vm.LoadFromEntity(o);
                        return vm;
                    }));

                OrdersList.ItemsSource = _orders;
                UpdateEmptyState();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading orders: {ex}");

                _orders.Clear();
                OrdersList.ItemsSource = _orders;
                UpdateEmptyState();
            }
        }

        private void UpdateEmptyState()
        {
            bool hasOrders = _orders.Any();
            EmptyStatePanel.Visibility = hasOrders ? Visibility.Collapsed : Visibility.Visible;
            OrdersList.Visibility = hasOrders ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ReviewButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            _selectedOrderForReview = button?.Tag as OrderViewModel;

            if (_selectedOrderForReview == null)
                return;

            var firstItem = _selectedOrderForReview.OrderItems.FirstOrDefault();
            if (firstItem == null)
            {
                ReviewStatusText.Text = "This order has no items to review.";
                return;
            }

            ReviewDialogTitle.Text = $"Review: {firstItem.MealProductName}";
            ResetStarRating();
            ReviewTextBox.Text = string.Empty;
            ReviewStatusText.Text = string.Empty;
            _ = ReviewDialog.ShowAsync();
        }

        private T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(child);

            while (parent != null)
            {
                if (parent is T typedParent)
                    return typedParent;

                parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(parent);
            }

            return null;
        }

        private async void ReorderButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var order = button?.Tag as OrderViewModel;

            if (order == null)
                return;

            var dialog = new ContentDialog
            {
                Title = "Reorder",
                Content = $"Items from order {order.OrderID} have been added to your cart.",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };

            await dialog.ShowAsync();

            var layout = FindParent<PESYONG.Presentation.Components.Layouts.CustomerLayout>(this);
            layout?.NavigateByTag("CartPage");
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
                    i < _currentRating
                        ? Windows.UI.Color.FromArgb(255, 255, 102, 0)
                        : Windows.UI.Color.FromArgb(255, 255, 178, 102));
            }
        }

        private async void ReviewDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
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

            var reviewData = new
            {
                OrderId = _selectedOrderForReview?.OrderID,
                Rating = _currentRating,
                Review = ReviewTextBox.Text.Trim(),
                Date = DateTime.Now
            };

            System.Diagnostics.Debug.WriteLine(
                $"Review submitted: Order={reviewData.OrderId}, Rating={reviewData.Rating}, Review={reviewData.Review}");

            var successDialog = new ContentDialog
            {
                Title = "Review Submitted",
                Content = "Thank you for your feedback!",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };

            await successDialog.ShowAsync();
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}