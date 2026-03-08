
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.ApplicationLogic.Services;
using PESYONG.Domain.Entities;
using PESYONG.Presentation.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PESYONG.Presentation.Views.Customer
{
    public sealed partial class CartPage : Page, INotifyPropertyChanged
    {
        private readonly CartService _cartService;
        private readonly OrderRepository _orderRepository;

        private ObservableCollection<CartItem> Cart => _cartService.Cart;
        private DeliveryInfo? Delivery => _cartService.Delivery;

        public CheckoutViewModel CheckoutVM { get; }

        private enum CheckoutStep { Cart, Delivery, Payment }
        private CheckoutStep _currentStep = CheckoutStep.Cart;

        public event PropertyChangedEventHandler? PropertyChanged;

        public CartPage()
        {
            this.InitializeComponent();

            _cartService = CartService.Instance;
            _orderRepository = App.Current.Services.GetRequiredService<OrderRepository>();
            CheckoutVM = new CheckoutViewModel(_orderRepository);

            DataContext = this;

            LoadCart();
        }

        private void LoadCart()
        {
            UpdateCartDisplay();
            UpdateOrderSummary();

            Cart.CollectionChanged += (s, e) =>
            {
                UpdateCartDisplay();
                UpdateOrderSummary();
                CheckoutVM.Initialize(Cart);
            };

            CheckoutVM.Initialize(Cart);
        }

        private void UpdateCartDisplay()
        {
            HeaderSubtitle.Text = Cart.Count == 0
                ? "Your cart is empty"
                : $"{Cart.Count} {(Cart.Count == 1 ? "item" : "items")} in your cart";

            EmptyCartPanel.Visibility = Cart.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            CartItemsPanel.Visibility = Cart.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            OrderSummaryPanel.Visibility = Cart.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

            CartItemsList.ItemsSource = null;
            CartItemsList.ItemsSource = Cart;
        }

        private void UpdateOrderSummary()
        {
            double subtotal = _cartService.GetSubtotal();
            double total = _cartService.GetTotal();

            SubtotalText.Text = $"₱{subtotal:F2}";
            TotalText.Text = $"₱{total:F2}";

            // Payment summary can now come from CheckoutVM bindings,
            // but leaving this here is okay while transitioning.
            PaymentSubtotalText.Text = CheckoutVM.SubtotalDisplay;
            PaymentTotalText.Text = CheckoutVM.TotalDisplay;
            PaymentDeliveryFeeText.Text = CheckoutVM.DeliveryFeeDisplay;

            if (Delivery != null)
            {
                DeliveryFeeText.Text = $"₱{Delivery.DeliveryFee:F2}";
            }
            else
            {
                DeliveryFeeText.Text = "Calculated at checkout";
            }
        }

        private void UpdateViewVisibility()
        {
            CartView.Visibility = _currentStep == CheckoutStep.Cart ? Visibility.Visible : Visibility.Collapsed;
            DeliveryView.Visibility = _currentStep == CheckoutStep.Delivery ? Visibility.Visible : Visibility.Collapsed;
            PaymentView.Visibility = _currentStep == CheckoutStep.Payment ? Visibility.Visible : Visibility.Collapsed;

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

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is string id)
            {
                var item = Cart.FirstOrDefault(i => i.Id == id);
                if (item != null)
                {
                    _cartService.UpdateQuantity(id, item.Quantity + 1);
                    CheckoutVM.Initialize(Cart);
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
                    CheckoutVM.Initialize(Cart);
                    UpdateOrderSummary();
                }
            }
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is string id)
            {
                _cartService.RemoveFromCart(id);
                CheckoutVM.Initialize(Cart);
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
            Frame.Navigate(typeof(CateringPackagesPage));
        }

        private void ProceedToCheckout_Click(object sender, RoutedEventArgs e)
        {
            CheckoutVM.Initialize(Cart);
            _currentStep = CheckoutStep.Delivery;
            UpdateViewVisibility();
        }

        private void LocationRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DistancePanel.Visibility = CheckoutVM.SelectedLocationIndex == 1
                ? Visibility.Visible
                : Visibility.Collapsed;

            EstimatedFeeText.Text = $"Estimated fee: {CheckoutVM.DeliveryFeeDisplay}";
        }

        private void DistanceTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EstimatedFeeText.Text = $"Estimated fee: {CheckoutVM.DeliveryFeeDisplay}";
        }

        private void BackToCart_Click(object sender, RoutedEventArgs e)
        {
            _currentStep = CheckoutStep.Cart;
            UpdateViewVisibility();
        }

        private void ContinueToPayment_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckoutVM.Validate(out string errorMessage))
            {
                ShowErrorDialog(errorMessage);
                return;
            }

            var deliveryInfo = new DeliveryInfo
            {
                Address = CheckoutVM.ShippingAddress,
                Location = CheckoutVM.Location,
                Distance = double.TryParse(CheckoutVM.DistanceText, out double distance) ? distance : null,
                DeliveryFee = CheckoutVM.DeliveryFee
            };

            _cartService.SetDelivery(deliveryInfo);
            UpdateOrderSummary();

            _currentStep = CheckoutStep.Payment;
            UpdateViewVisibility();
        }

        private void BackToDelivery_Click(object sender, RoutedEventArgs e)
        {
            _currentStep = CheckoutStep.Delivery;
            UpdateViewVisibility();
        }


        private async void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckoutVM.Validate(out string errorMessage))
                {
                    ShowErrorDialog(errorMessage);
                    return;
                }

                if (Cart.Count == 0)
                {
                    ShowErrorDialog("Your cart is empty.");
                    return;
                }

                var orderId = await CheckoutVM.SubmitOrderAsync();

                if (orderId == null)
                {
                    ShowErrorDialog("Failed to place order.");
                    return;
                }

                var orderNumber = orderId.Value.ToString()[..8].ToUpper();

                var dialog = new ContentDialog
                {
                    Title = "Order Placed Successfully! 🎉",
                    Content = $"Order #{orderNumber} has been received.",
                    PrimaryButtonText = "View Orders",
                    CloseButtonText = "Continue Shopping",
                    XamlRoot = this.XamlRoot
                };

                dialog.PrimaryButtonClick += (s, args) =>
                {
                    _cartService.ClearCart();
                    CheckoutVM.ClearForm();
                    Frame.Navigate(typeof(OrderHistoryPage));
                };

                dialog.CloseButtonClick += (s, args) =>
                {
                    _cartService.ClearCart();
                    CheckoutVM.ClearForm();
                    _currentStep = CheckoutStep.Cart;
                    UpdateViewVisibility();
                    UpdateCartDisplay();
                    UpdateOrderSummary();
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                var root = ex.GetBaseException();
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                await new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to place order:\n{root.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
            }
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