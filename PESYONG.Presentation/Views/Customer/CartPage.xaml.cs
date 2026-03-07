using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using PESYONG.ApplicationLogic.Services;
using PESYONG.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PESYONG.Presentation.Views.Customer
{
    /// <summary>
    /// Manages the multi-step checkout process, including cart review, 
    /// delivery information gathering, and payment method selection.
    /// Coordinates between the <see cref="CartService"/> and the UI state 
    /// to ensure accurate order totals and validation.
    /// </summary>
    /// 
    public sealed partial class CartPage : Page, INotifyPropertyChanged
    {
        private readonly CartService _cartService;
        private ObservableCollection<CartItem> Cart => _cartService.Cart;
        private DeliveryInfo? Delivery => _cartService.Delivery;

        private enum CheckoutStep { Cart, Delivery, Payment }
        private CheckoutStep _currentStep = CheckoutStep.Cart;

        public event PropertyChangedEventHandler? PropertyChanged;

        public CartPage()
        {
            this.InitializeComponent();
            _cartService = CartService.Instance;
            LoadCart();
        }

        private void LoadCart()
        {
            UpdateCartDisplay();
            UpdateOrderSummary();

            // Subscribe to cart changes
            Cart.CollectionChanged += (s, e) => UpdateCartDisplay();
        }

        /// <summary>
        /// Synchronizes the UI elements with the current state of the cart, 
        /// toggling visibility between the empty-state panel and the active items list.
        /// </summary>
        private void UpdateCartDisplay()
        {
            // Update header subtitle
            HeaderSubtitle.Text = Cart.Count == 0
                ? "Your cart is empty"
                : $"{Cart.Count} {(Cart.Count == 1 ? "item" : "items")} in your cart";

            // Show/hide empty cart panel
            EmptyCartPanel.Visibility = Cart.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            CartItemsPanel.Visibility = Cart.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            OrderSummaryPanel.Visibility = Cart.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

            // Refresh cart items list
            CartItemsList.ItemsSource = null;
            CartItemsList.ItemsSource = Cart;
        }

        private void UpdateOrderSummary()
        {
            double subtotal = _cartService.GetSubtotal();
            double total = _cartService.GetTotal();

            SubtotalText.Text = $"₱{subtotal:F2}";
            TotalText.Text = $"₱{total:F2}";

            PaymentSubtotalText.Text = $"₱{subtotal:F2}";
            PaymentTotalText.Text = $"₱{total:F2}";

            if (Delivery != null)
            {
                PaymentDeliveryFeeText.Text = $"₱{Delivery.DeliveryFee:F2}";
                DeliveryFeeText.Text = $"₱{Delivery.DeliveryFee:F2}";
            }
        }

        private void UpdateViewVisibility()
        {
            CartView.Visibility = _currentStep == CheckoutStep.Cart ? Visibility.Visible : Visibility.Collapsed;
            DeliveryView.Visibility = _currentStep == CheckoutStep.Delivery ? Visibility.Visible : Visibility.Collapsed;
            PaymentView.Visibility = _currentStep == CheckoutStep.Payment ? Visibility.Visible : Visibility.Collapsed;

            // Update header based on step
            switch (_currentStep)
            {
                case CheckoutStep.Cart:
                    HeaderTitle.Text = "My Cart";
                    break;
                case CheckoutStep.Delivery:
                    HeaderTitle.Text = "Delivery Details";
                    break;
                case CheckoutStep.Payment:
                    HeaderTitle.Text = "Payment Method";
                    break;
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Cart View Event Handlers
        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is string id)
            {
                var item = Cart.FirstOrDefault(i => i.Id == id);
                if (item != null)
                {
                    _cartService.UpdateQuantity(id, item.Quantity + 1);
                    UpdateOrderSummary();
                }
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is string id)
            {
                var item = Cart.FirstOrDefault(i => i.Id == id);
                if (item != null)
                {
                    _cartService.UpdateQuantity(id, item.Quantity - 1);
                    UpdateOrderSummary();
                }
            }
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is string id)
            {
                _cartService.RemoveFromCart(id);
                UpdateOrderSummary();

                var dialog = new ContentDialog
                {
                    Title = "Item Removed",
                    Content = "Item has been removed from your cart.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = dialog.ShowAsync();
            }
        }

        private void BrowsePackages_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to packages page
            Frame.Navigate(typeof(CateringPackagesPage));
        }

        private void ProceedToCheckout_Click(object sender, RoutedEventArgs e)
        {
            _currentStep = CheckoutStep.Delivery;
            UpdateViewVisibility();
        }

        // Delivery View Event Handlers
        private void LocationRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var radioButtons = sender as RadioButtons;
            DistancePanel.Visibility = radioButtons?.SelectedIndex == 1
                ? Visibility.Visible
                : Visibility.Collapsed;

            UpdateEstimatedFee();
        }

        private void DistanceTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateEstimatedFee();
        }

        private void UpdateEstimatedFee()
        {
            if (LocationRadioButtons.SelectedIndex == 0)
            {
                EstimatedFeeText.Text = "Estimated fee: ₱15.00";
            }
            else if (double.TryParse(DistanceTextBox.Text, out double distance))
            {
                double fee = Math.Max(25, Math.Floor(distance) * 5);
                EstimatedFeeText.Text = $"Estimated fee: ₱{fee:F2}";
            }
        }

        private void BackToCart_Click(object sender, RoutedEventArgs e)
        {
            _currentStep = CheckoutStep.Cart;
            UpdateViewVisibility();
        }

        /// <summary>
        /// Validates the delivery input fields and distance calculations 
        /// before transitioning the UI to the payment selection step.
        /// </summary>
        private void ContinueToPayment_Click(object sender, RoutedEventArgs e)
        {
            // Validate delivery form
            if (string.IsNullOrWhiteSpace(AddressTextBox.Text))
            {
                ShowErrorDialog("Please enter a delivery address.");
                return;
            }

            if (DeliveryDatePicker.Date == null)
            {
                ShowErrorDialog("Please select a delivery date.");
                return;
            }

            if (LocationRadioButtons.SelectedIndex == 1 &&
                !double.TryParse(DistanceTextBox.Text, out _))
            {
                ShowErrorDialog("Please enter a valid distance for outside Poblacion delivery.");
                return;
            }

            // Calculate delivery fee
            double deliveryFee = LocationRadioButtons.SelectedIndex == 0
                ? 15
                : Math.Max(25, Math.Floor(double.Parse(DistanceTextBox.Text)) * 5);

            // Create delivery info
            var deliveryInfo = new DeliveryInfo
            {
                Address = AddressTextBox.Text,
                Location = LocationRadioButtons.SelectedIndex == 0 ? "poblacion" : "outside",
                Distance = LocationRadioButtons.SelectedIndex == 1 ? double.Parse(DistanceTextBox.Text) : null,
                DeliveryFee = deliveryFee
            };

            _cartService.SetDelivery(deliveryInfo);
            UpdateOrderSummary();

            _currentStep = CheckoutStep.Payment;
            UpdateViewVisibility();
        }

        // Payment View Event Handlers
        private void BackToDelivery_Click(object sender, RoutedEventArgs e)
        {
            _currentStep = CheckoutStep.Delivery;
            UpdateViewVisibility();
        }

        /// <summary>
        /// Finalizes the transaction by capturing the selected payment method, 
        /// clearing the cart, and prompting the user for navigation to their order history.
        /// </summary>
        private async void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            string paymentMethod = PaymentRadioButtons.SelectedIndex switch
            {
                0 => "cod",
                1 => "gcash",
                2 => "reservation",
                _ => "cod"
            };

            // Create order (this would call a service)
            var orderData = new
            {
                Id = $"ORD-{DateTime.Now.Ticks}",
                Items = Cart.ToList(),
                DeliveryInfo = Delivery,
                PaymentMethod = paymentMethod,
                Total = _cartService.GetTotal(),
                Status = "pending",
                Date = DateTime.Now
            };

            // Show success dialog
            var dialog = new ContentDialog
            {
                Title = "Order Placed Successfully! 🎉",
                Content = $"Order #{orderData.Id} has been received.",
                PrimaryButtonText = "View Orders",
                CloseButtonText = "Continue Shopping",
                XamlRoot = this.XamlRoot
            };

            dialog.PrimaryButtonClick += (s, args) =>
            {
                // Navigate to orders page
                Frame.Navigate(typeof(OrderHistoryPage));
            };

            dialog.CloseButtonClick += (s, args) =>
            {
                _cartService.ClearCart();
                _currentStep = CheckoutStep.Cart;
                UpdateViewVisibility();
                UpdateCartDisplay();
            };

            await dialog.ShowAsync();
        }

        private void ShowErrorDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Validation Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            _ = dialog.ShowAsync();
        }


    }

}