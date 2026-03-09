using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.Presentation.ViewModels.ObjectModels;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using WinRT.Interop;

namespace PESYONG.Presentation.Views.Admin.Meals;

public sealed partial class MealProductPage : Page
{
    private readonly MealProductRepository _mealProductRepository;
    private readonly MealRepository _mealRepository;
    private bool _isLoading;

    public ObservableCollection<MealProductViewModel> MealProductListViewModels { get; } = new();

    private MealProductViewModel? CurrentViewModel => DataContext as MealProductViewModel;
    private byte[]? _selectedImageBytes;

    public MealProductPage()
    {
        InitializeComponent();

        try
        {
            _mealProductRepository = App.Current.Services.GetRequiredService<MealProductRepository>();
            _mealRepository = App.Current.Services.GetRequiredService<MealRepository>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MealProductPage service resolution failed: {ex}");
            throw;
        }

        Loaded += MealProductPage_Loaded;
    }

    private async void MealProductPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_isLoading)
            return;

        try
        {
            _isLoading = true;
            await RefreshPageAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MealProductPage load failed: {ex}");
            SetStatus("Unable to load catering packages.");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task RefreshPageAsync()
    {
        try
        {
            var selectedId = CurrentViewModel?.MealProductID;

            var mealProducts = (await _mealProductRepository.GetAllMealProductsAsync())
                .Where(x => x.IsCateringPackage)
                .GroupBy(x => x.MealProductID)
                .Select(g => g.First())
                .OrderBy(x => x.MealProductID)
                .ToList();

            var meals = await _mealRepository.GetAllMealsAsync();

            MealProductListViewModels.Clear();

            foreach (var entity in mealProducts)
            {
                var vm = MealProductViewModel.CreateFromEntity(entity);
                vm.SetAvailableMeals(meals);
                MealProductListViewModels.Add(vm);
            }

            if (MealProductListViewModels.Count == 0)
            {
                var emptyVm = CreateDefaultViewModel();
                emptyVm.SetAvailableMeals(meals);
                emptyVm.StatusMessage = "No catering packages found. Create a new one.";
                DataContext = emptyVm;
                MealProductsListView.SelectedItem = null;
                return;
            }

            var selectedVm = selectedId.HasValue
                ? MealProductListViewModels.FirstOrDefault(x => x.MealProductID == selectedId.Value)
                : MealProductListViewModels.FirstOrDefault();

            selectedVm ??= MealProductListViewModels.First();
            selectedVm.SetAvailableMeals(meals);

            if (selectedVm.MealProductID.HasValue)
            {
                var fullEntity = await _mealProductRepository.GetMealProductByIdAsync(selectedVm.MealProductID.Value);
                _selectedImageBytes = fullEntity?.ImageBytes;
                selectedVm.ImageBytes = fullEntity?.ImageBytes;
            }
            else
            {
                _selectedImageBytes = null;
                selectedVm.ImageBytes = null;
            }

            MealProductsListView.SelectedItem = selectedVm;
            DataContext = selectedVm;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"RefreshPageAsync error: {ex}");
            SetStatus("Unable to refresh catering package data.");
        }
    }

    private MealProductViewModel CreateDefaultViewModel()
    {
        var vm = new MealProductViewModel();
        vm.ClearMealProductViewModel();
        vm.IsCateringPackage = true;
        vm.IsAvailable = true;
        vm.CreationDate = DateTime.UtcNow;
        vm.LastModifiedDate = DateTime.UtcNow;
        return vm;
    }

    private async void MealProductsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoading)
            return;

        if (MealProductsListView.SelectedItem is not MealProductViewModel selectedVm)
            return;

        try
        {
            if (selectedVm.MealProductID.HasValue)
            {
                var entity = await _mealProductRepository.GetMealProductByIdAsync(selectedVm.MealProductID.Value);
                if (entity != null)
                {
                    var meals = await _mealRepository.GetAllMealsAsync();
                    selectedVm.LoadFromEntity(entity);
                    _selectedImageBytes = entity.ImageBytes;
                    selectedVm.ImageBytes = entity.ImageBytes;
                    selectedVm.SetAvailableMeals(meals);
                }
            }

            DataContext = selectedVm;
            UpdateCustomizableUI(selectedVm.IsCustomizable);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Selection changed error: {ex}");
            selectedVm.StatusMessage = "Unable to load selected catering package.";
        }
    }

    private async void AddMealProductButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var meals = await _mealRepository.GetAllMealsAsync();

            var vm = CreateDefaultViewModel();
            vm.SetAvailableMeals(meals);
            vm.StatusMessage = "New catering package draft created.";

            DataContext = vm;
            MealProductsListView.SelectedItem = null;
            _selectedImageBytes = null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AddMealProductButton_Click error: {ex}");
            SetStatus("Unable to create a new catering package draft.");
        }
    }

    private void FormTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (CurrentViewModel == null)
            return;

        CurrentViewModel.StatusMessage = string.Empty;
        CurrentViewModel.ValidateAll();
    }

    private void AddMealItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentViewModel == null)
            return;

        try
        {
            if (CurrentViewModel.SelectedMealForAdd is null)
            {
                CurrentViewModel.StatusMessage = "Select a meal before adding.";
                return;
            }

            UpdateCustomizableUI(false);

            CurrentViewModel.AddMealItem(CurrentViewModel.SelectedMealForAdd);
            CurrentViewModel.StatusMessage = $"Added {CurrentViewModel.SelectedMealForAdd.MealName} to the package.";
            CurrentViewModel.ValidateAll();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AddMealItemButton_Click error: {ex}");
            CurrentViewModel.StatusMessage = "Unable to add meal to package.";
        }
    }

    private void RemoveMealItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentViewModel == null)
            return;

        try
        {
            if (sender is Button button && button.DataContext is MealProductItemViewModel itemVm)
            {
                CurrentViewModel.RemoveMealItem(itemVm);
                CurrentViewModel.StatusMessage = "Meal removed from package.";
                CurrentViewModel.ValidateAll();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"RemoveMealItemButton_Click error: {ex}");
            CurrentViewModel.StatusMessage = "Unable to remove meal from package.";
        }
    }

    private async void SaveMealProductButton_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentViewModel == null)
            return;

        try
        {
            CurrentViewModel.StatusMessage = string.Empty;
            CurrentViewModel.LastModifiedDate = DateTime.UtcNow;

            if (!CurrentViewModel.ValidateAll())
            {
                CurrentViewModel.StatusMessage = "Please fix validation errors before saving the catering package.";
                return;
            }

            if (CurrentViewModel.IsCustomizable)
            {
                if (CurrentViewModel.PreferredViandCount <= 0)
                {
                    CurrentViewModel.StatusMessage = "Preferred viand count must be greater than 0 for customizable packages.";
                    return;
                }
            }
            else
            {
                if (CurrentViewModel.MealProductItems == null || !CurrentViewModel.MealProductItems.Any())
                {
                    CurrentViewModel.StatusMessage = "Please add at least one meal for a fixed package.";
                    return;
                }
            }

            var entity = CurrentViewModel.ToEntity();

            if (_selectedImageBytes != null)
            {
                entity.ImageBytes = _selectedImageBytes;
            }
            else if (CurrentViewModel.MealProductID.HasValue)
            {
                var existing = await _mealProductRepository.GetMealProductByIdAsync(CurrentViewModel.MealProductID.Value);
                if (existing != null)
                {
                    entity.ImageBytes = existing.ImageBytes;
                }
            }

            if (CurrentViewModel.MealProductID.HasValue)
            {
                Debug.WriteLine($"Updating catering package ID {CurrentViewModel.MealProductID.Value}");
                await _mealProductRepository.UpdateMealProductAsync(entity);
            }
            else
            {
                Debug.WriteLine("Creating new catering package");
                var created = await _mealProductRepository.CreateMealProductAsyncReturnSelf(entity);
                CurrentViewModel.LoadFromEntity(created);
            }

            var savedId = CurrentViewModel.MealProductID;
            await RefreshPageAsync();

            var refreshed = savedId.HasValue
                ? MealProductListViewModels.FirstOrDefault(x => x.MealProductID == savedId.Value)
                : null;

            if (refreshed != null)
            {
                MealProductsListView.SelectedItem = refreshed;
                DataContext = refreshed;
                refreshed.StatusMessage = "Catering package saved successfully.";
                UpdateCustomizableUI(refreshed.IsCustomizable);
            }
        }
        catch (InvalidOperationException ex)
        {
            Debug.WriteLine($"Save validation error: {ex}");
            CurrentViewModel.StatusMessage = ex.Message;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SaveMealProductButton_Click error: {ex}");
            CurrentViewModel.StatusMessage = "Failed to save catering package. Check logs for details.";
        }
    }

    private async void DeleteMealProductButton_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentViewModel == null || !CurrentViewModel.MealProductID.HasValue)
        {
            SetStatus("Delete skipped. Select a saved catering package first.");
            return;
        }

        try
        {
            var id = CurrentViewModel.MealProductID.Value;
            await _mealProductRepository.DeleteMealProductAsync(id);

            await RefreshPageAsync();
            SetStatus("Catering package deleted successfully.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"DeleteMealProductButton_Click error: {ex}");
            CurrentViewModel.StatusMessage = "Delete failed. Check logs for details.";
        }
    }

    private void SetStatus(string message)
    {
        if (CurrentViewModel != null)
        {
            CurrentViewModel.StatusMessage = message;
        }
    }

    private void PaxCountNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (CurrentViewModel == null)
            return;

        CurrentViewModel.StatusMessage = string.Empty;
        CurrentViewModel.ValidateAll();
    }

    private async void PickImageButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await PickImageAsync();

            if (CurrentViewModel != null && _selectedImageBytes != null)
            {
                CurrentViewModel.ImageBytes = null;
                CurrentViewModel.ImageBytes = _selectedImageBytes;
                CurrentViewModel.StatusMessage = "Package image selected successfully.";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PickImageButton_Click error: {ex}");
            SetStatus("Unable to select package image.");
        }
    }

    private async Task PickImageAsync()
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");

        var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(picker, hwnd);

        StorageFile? file = await picker.PickSingleFileAsync();
        if (file == null)
            return;

        using var stream = await file.OpenReadAsync();
        _selectedImageBytes = new byte[stream.Size];
        await stream.ReadAsync(_selectedImageBytes.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);

        Debug.WriteLine($"Selected package image bytes: {_selectedImageBytes.Length}");
    }

    private void IsCustomizableCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        UpdateCustomizableUI(true);
    }

    private void IsCustomizableCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        UpdateCustomizableUI(false);
    }

    private void UpdateCustomizableUI(bool isCustomizable)
    {
        PreferredViandCountPanel.Visibility = isCustomizable
            ? Visibility.Visible
            : Visibility.Collapsed;

        CustomizableHintText.Visibility = isCustomizable
            ? Visibility.Visible
            : Visibility.Collapsed;

        MealSelectorComboBox.IsEnabled = !isCustomizable;
        AddMealButton.IsEnabled = !isCustomizable;
        MealItemsListView.IsEnabled = !isCustomizable;

        MealsSectionTitle.Text = isCustomizable
            ? "Fixed Meals Disabled"
            : "Meals to Include in Package";

        IncludedMealsTitle.Text = isCustomizable
            ? "Fixed Meals Not Required"
            : "Meals Included in This Package";
    }
}