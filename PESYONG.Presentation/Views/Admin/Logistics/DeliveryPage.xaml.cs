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

namespace PESYONG.Presentation.Views.Admin.Logistics;

public sealed partial class DeliveryPage : Page
{
    private readonly DeliveryRepository _deliveryRepository;
    private bool _isLoading;

    public ObservableCollection<DeliveryViewModel> DeliveryListViewModels { get; } = new();

    private DeliveryViewModel? SelectedDeliveryViewModel => DataContext as DeliveryViewModel;

    public Array DeliveryStatuses { get; } = Enum.GetValues(typeof(DeliveryStatus));

    public DeliveryPage()
    {
        InitializeComponent();

        try
        {
            _deliveryRepository = App.Current.Services.GetRequiredService<DeliveryRepository>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Service resolution failed: {ex}");
            throw;
        }

        Loaded += DeliveryPage_Loaded;
    }

    private async void DeliveryPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_isLoading)
            return;

        try
        {
            _isLoading = true;
            await RefreshDeliveryListAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Page load failed: {ex}");
            SetStatus("Unable to load deliveries. Check logs for details.");
        }
        finally
        {
            _isLoading = false;
        }
    }

    // -----------------------------
    // Data Loading
    // -----------------------------
    private async Task RefreshDeliveryListAsync()
    {
        try
        {
            var selectedId = SelectedDeliveryViewModel?.DeliveryID;

            DeliveryListViewModels.Clear();

            var allDeliveries = await _deliveryRepository.GetAllDeliveriesAsync();
            Debug.WriteLine($"Loaded {allDeliveries.Count} deliveries from database.");

            foreach (var delivery in allDeliveries.OrderBy(x => x.DeliveryID))
            {
                DeliveryListViewModels.Add(DeliveryViewModel.CreateFromEntity(delivery));
            }

            if (DeliveryListViewModels.Count == 0)
            {
                var emptyVm = CreateDefaultDeliveryViewModel();
                emptyVm.StatusMessage = "No deliveries found. Create a new one.";
                DataContext = emptyVm;
                DeliveriesListView.SelectedItem = null;
                return;
            }

            var selectedVm = selectedId.HasValue
                ? DeliveryListViewModels.FirstOrDefault(x => x.DeliveryID == selectedId.Value)
                : DeliveryListViewModels.FirstOrDefault();

            selectedVm ??= DeliveryListViewModels.First();

            DeliveriesListView.SelectedItem = selectedVm;
            DataContext = selectedVm;

            if (selectedVm.DeliveryID.HasValue)
            {
                var fullDelivery = await _deliveryRepository.GetDeliveryByIdAsync(selectedVm.DeliveryID.Value);
                if (fullDelivery != null)
                {
                    selectedVm.LoadFromEntity(fullDelivery);
                }
            }

            selectedVm.StatusMessage = string.Empty;
            selectedVm.RefreshComputedState();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Refresh failed: {ex}");
            SetStatus("Unable to refresh the delivery list.");
        }
    }

    private DeliveryViewModel CreateDefaultDeliveryViewModel()
    {
        var vm = new DeliveryViewModel();
        vm.ClearDeliveryViewModel();
        vm.CreatedDate = DateTime.Now;
        vm.Status = DeliveryStatus.Pending;
        vm.SignatureRequired = true;
        return vm;
    }

    // -----------------------------
    // CRUD
    // -----------------------------
    private void AddDeliveryButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var newVm = CreateDefaultDeliveryViewModel();
            DataContext = newVm;
            DeliveriesListView.SelectedItem = null;

            newVm.StatusMessage = "New draft delivery created.";
            Debug.WriteLine("Created new draft delivery.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Add draft failed: {ex}");
            SetStatus("Unable to create a new delivery draft.");
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not DeliveryViewModel vm)
            return;

        try
        {
            vm.StatusMessage = string.Empty;

            if (!vm.ValidateAll())
            {
                vm.StatusMessage = "Please fix the validation errors before saving.";
                Debug.WriteLine("Save skipped: validation failed.");
                return;
            }

            var entity = vm.ToEntity();

            if (vm.DeliveryID.HasValue)
            {
                Debug.WriteLine($"Updating delivery ID {vm.DeliveryID.Value}...");
                await _deliveryRepository.UpdateDeliveryAsync(entity);
            }
            else
            {
                Debug.WriteLine("Creating new delivery...");
                var created = await _deliveryRepository.CreateDeliveryAsyncReturnSelf(entity);
                vm.LoadFromEntity(created);
            }

            var targetId = vm.DeliveryID;
            await RefreshDeliveryListAsync();

            if (targetId.HasValue)
            {
                var refreshedVm = DeliveryListViewModels.FirstOrDefault(x => x.DeliveryID == targetId.Value);
                if (refreshedVm != null)
                {
                    DeliveriesListView.SelectedItem = refreshedVm;
                    DataContext = refreshedVm;
                    refreshedVm.StatusMessage = "Delivery saved successfully.";
                }
            }

            Debug.WriteLine("Delivery saved successfully.");
        }
        catch (InvalidOperationException ex)
        {
            Debug.WriteLine($"Validation save block: {ex}");
            vm.StatusMessage = ex.Message;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Save failed: {ex}");
            vm.StatusMessage = "Save failed. Check logs for details.";
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not DeliveryViewModel vm || !vm.DeliveryID.HasValue)
        {
            Debug.WriteLine("Delete skipped: no saved delivery selected.");
            SetStatus("Delete skipped. Select a saved delivery first.");
            return;
        }

        try
        {
            Debug.WriteLine($"Deleting delivery ID {vm.DeliveryID.Value}...");
            await _deliveryRepository.DeleteDeliveryAsync(vm.DeliveryID.Value);

            await RefreshDeliveryListAsync();
            SetStatus("Delivery deleted successfully.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Delete failed: {ex}");
            vm.StatusMessage = "Delete failed. Check logs for details.";
        }
    }

    private void AddDeliveryUpdateButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not DeliveryViewModel vm)
            return;

        try
        {
            if (!vm.DeliveryID.HasValue)
            {
                vm.StatusMessage = "Save the delivery first before adding updates.";
                return;
            }

            var updateVm = new DeliveryUpdateViewModel
            {
                DeliveryID = vm.DeliveryID.Value,
                Status = vm.Status,
                UpdateDate = DateTime.Now,
                UpdateDescription = "New delivery update"
            };

            vm.DeliveryUpdates.Insert(0, updateVm);
            vm.StatusMessage = "New delivery update draft added.";
            Debug.WriteLine("Delivery update draft added.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Add delivery update failed: {ex}");
            vm.StatusMessage = "Unable to add a delivery update draft.";
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        _ = RefreshDeliveryListAsync();
    }

    // -----------------------------
    // UI Events
    // -----------------------------
    private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoading)
            return;

        if (sender is not ListView listView)
            return;

        if (listView.SelectedItem is not DeliveryViewModel selectedVm)
            return;

        try
        {
            DataContext = selectedVm;

            if (selectedVm.DeliveryID.HasValue)
            {
                var fullDelivery = await _deliveryRepository.GetDeliveryByIdAsync(selectedVm.DeliveryID.Value);
                if (fullDelivery != null)
                {
                    selectedVm.LoadFromEntity(fullDelivery);
                }
            }

            selectedVm.RefreshComputedState();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Selection change failed: {ex}");
            selectedVm.StatusMessage = "Unable to load the selected delivery details.";
        }
    }

    private void FormFieldChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is DeliveryViewModel vm)
        {
            vm.StatusMessage = string.Empty;
            vm.ValidateAll();
            vm.RefreshComputedState();
        }
    }

    private void NumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (DataContext is DeliveryViewModel vm)
        {
            vm.StatusMessage = string.Empty;
            vm.ValidateAll();
            vm.RefreshComputedState();
        }
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is DeliveryViewModel vm)
        {
            vm.StatusMessage = string.Empty;
            vm.ValidateAll();
            vm.RefreshComputedState();
        }
    }

    private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (DataContext is DeliveryViewModel vm)
        {
            vm.StatusMessage = string.Empty;
            vm.ValidateAll();
            vm.RefreshComputedState();
        }
    }

    private void SetStatus(string message)
    {
        if (DataContext is DeliveryViewModel vm)
        {
            vm.StatusMessage = message;
        }
    }
}