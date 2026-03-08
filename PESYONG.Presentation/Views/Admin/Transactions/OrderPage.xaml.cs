using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.Domain.Enums;
using PESYONG.Presentation.ViewModels.ObjectModels;

namespace PESYONG.Presentation.Views.Admin.Transactions;

public sealed partial class OrderPage : Page
{
    private readonly OrderRepository _orderRepository;
    private bool _isLoading;

    public ObservableCollection<OrderViewModel> OrderListViewModels { get; } = new();

    public Array DeliveryStatuses { get; } = Enum.GetValues(typeof(DeliveryStatus));

    public OrderPage()
    {
        InitializeComponent();

        try
        {
            _orderRepository = App.Current.Services.GetRequiredService<OrderRepository>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Service resolution failed: {ex}");
            throw;
        }

        Loaded += OrderPage_Loaded;
    }

    private async void OrderPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_isLoading)
            return;

        try
        {
            _isLoading = true;
            await RefreshOrderListAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Order page load failed: {ex}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task RefreshOrderListAsync()
    {
        try
        {
            var selectedId = (DataContext as OrderViewModel)?.OrderID;

            OrderListViewModels.Clear();

            var orders = await _orderRepository.GetAllOrdersAsync();
            foreach (var order in orders.OrderByDescending(x => x.OrderDate))
            {
                OrderListViewModels.Add(OrderViewModel.CreateFromEntity(order));
            }

            if (OrderListViewModels.Count == 0)
            {
                var emptyVm = CreateDefaultOrderViewModel();
                emptyVm.StatusMessage = "No orders found. Create a new one.";
                DataContext = emptyVm;
                OrdersListView.SelectedItem = null;
                return;
            }

            var selectedVm = selectedId != Guid.Empty
                ? OrderListViewModels.FirstOrDefault(x => x.OrderID == selectedId)
                : OrderListViewModels.FirstOrDefault();

            selectedVm ??= OrderListViewModels.First();

            OrdersListView.SelectedItem = selectedVm;
            DataContext = selectedVm;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Refresh orders failed: {ex}");
        }
    }

    private OrderViewModel CreateDefaultOrderViewModel()
    {
        var vm = new OrderViewModel();
        vm.Clear();
        return vm;
    }

    private void AddOrderButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var vm = CreateDefaultOrderViewModel();
            DataContext = vm;
            OrdersListView.SelectedItem = null;
            vm.StatusMessage = "New draft order created.";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Add order failed: {ex}");
        }
    }

    private async void OrdersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoading)
            return;

        if (OrdersListView.SelectedItem is not OrderViewModel vm)
            return;

        try
        {
            DataContext = vm;

            if (vm.OrderID != Guid.Empty)
            {
                var entity = await _orderRepository.GetOrderByIdAsync(vm.OrderID);
                if (entity != null)
                {
                    vm.LoadFromEntity(entity);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Order selection failed: {ex}");
            vm.StatusMessage = "Unable to load selected order.";
        }
    }

    private void FormTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is OrderViewModel vm)
        {
            vm.ValidateAll();
            vm.RefreshTotals();
        }
    }

    private void FormComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is OrderViewModel vm)
        {
            vm.ValidateAll();
        }
    }

    private void AddOrderItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not OrderViewModel vm)
            return;

        try
        {
            if (!int.TryParse(MealProductIdTextBox.Text?.Trim(), out var mealProductId) || mealProductId <= 0)
            {
                vm.StatusMessage = "Enter a valid Meal Product ID.";
                return;
            }

            if (!decimal.TryParse(ItemPriceTextBox.Text?.Trim(), out var itemPrice) || itemPrice < 0)
            {
                vm.StatusMessage = "Enter a valid item price.";
                return;
            }

            var quantity = (int)ItemQuantityNumberBox.Value;
            if (quantity <= 0)
            {
                vm.StatusMessage = "Quantity must be at least 1.";
                return;
            }

            var itemVm = new OrderMealProductViewModel
            {
                OrderID = vm.OrderID,
                MealProductID = mealProductId,
                MealProductName = $"Meal Product #{mealProductId}",
                ItemPrice = itemPrice,
                MealProductOrderQty = quantity
            };

            vm.OrderItems.Add(itemVm);
            vm.RefreshTotals();
            vm.ValidateAll();

            MealProductIdTextBox.Text = string.Empty;
            ItemPriceTextBox.Text = string.Empty;
            ItemQuantityNumberBox.Value = 1;

            vm.StatusMessage = "Order item added.";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Add order item failed: {ex}");
            vm.StatusMessage = "Unable to add order item.";
        }
    }

    private void RemoveOrderItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not OrderViewModel vm)
            return;

        try
        {
            if (sender is Button button && button.DataContext is OrderMealProductViewModel itemVm)
            {
                vm.OrderItems.Remove(itemVm);
                vm.RefreshTotals();
                vm.ValidateAll();
                vm.StatusMessage = "Order item removed.";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Remove order item failed: {ex}");
            vm.StatusMessage = "Unable to remove order item.";
        }
    }

    private async void SaveOrderButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not OrderViewModel vm)
            return;

        try
        {
            if (!vm.ValidateAll())
            {
                vm.StatusMessage = "Please fix validation errors before saving.";
                return;
            }

            var entity = vm.ToEntity();

            if (vm.OrderID != Guid.Empty)
            {
                await _orderRepository.UpdateOrderAsync(entity);
            }
            else
            {
                var created = await _orderRepository.CreateOrderAsyncReturnSelf(entity);
                vm.LoadFromEntity(created);
            }

            await RefreshOrderListAsync();
            vm.StatusMessage = "Order saved successfully.";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Save order failed: {ex}");
            vm.StatusMessage = "Save failed. Check logs for details.";
        }
    }

    private async void DeleteOrderButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not OrderViewModel vm || vm.OrderID == Guid.Empty)
        {
            return;
        }

        try
        {
            await _orderRepository.DeleteOrderAsync(vm.OrderID);
            await RefreshOrderListAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Delete order failed: {ex}");
            vm.StatusMessage = "Delete failed. Check logs for details.";
        }
    }
}