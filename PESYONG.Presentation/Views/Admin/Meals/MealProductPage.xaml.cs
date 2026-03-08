using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.Presentation.ViewModels.ObjectModels;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PESYONG.Presentation.Views.Admin.Meals;

public sealed partial class MealProductPage : Page
{
    private readonly MealProductRepository _mealProductRepository;
    private readonly MealRepository _mealRepository;
    private bool _isLoading;

    public ObservableCollection<MealProductViewModel> MealProductListViewModels { get; } = new();

    private MealProductViewModel? CurrentViewModel => DataContext as MealProductViewModel;

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
            SetStatus("Unable to load meal products.");
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

            var mealProducts = await _mealProductRepository.GetAllMealProductsAsync();
            var meals = await _mealRepository.GetAllMealsAsync();

            MealProductListViewModels.Clear();

            foreach (var entity in mealProducts.OrderBy(x => x.MealProductID))
            {
                var vm = MealProductViewModel.CreateFromEntity(entity);
                vm.SetAvailableMeals(meals);
                MealProductListViewModels.Add(vm);
            }

            if (MealProductListViewModels.Count == 0)
            {
                var emptyVm = CreateDefaultViewModel();
                emptyVm.SetAvailableMeals(meals);
                emptyVm.StatusMessage = "No meal products found. Create a new one.";
                DataContext = emptyVm;
                MealProductsListView.SelectedItem = null;
                return;
            }

            var selectedVm = selectedId.HasValue
                ? MealProductListViewModels.FirstOrDefault(x => x.MealProductID == selectedId.Value)
                : MealProductListViewModels.FirstOrDefault();

            selectedVm ??= MealProductListViewModels.First();
            selectedVm.SetAvailableMeals(meals);

            MealProductsListView.SelectedItem = selectedVm;
            DataContext = selectedVm;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"RefreshPageAsync error: {ex}");
            SetStatus("Unable to refresh meal product data.");
        }
    }

    private MealProductViewModel CreateDefaultViewModel()
    {
        var vm = new MealProductViewModel();
        vm.ClearMealProductViewModel();
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
                    selectedVm.SetAvailableMeals(meals);
                }
            }

            DataContext = selectedVm;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Selection changed error: {ex}");
            selectedVm.StatusMessage = "Unable to load selected meal product.";
        }
    }

    private async void AddMealProductButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var meals = await _mealRepository.GetAllMealsAsync();

            var vm = CreateDefaultViewModel();
            vm.SetAvailableMeals(meals);
            vm.StatusMessage = "New meal product draft created.";

            DataContext = vm;
            MealProductsListView.SelectedItem = null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AddMealProductButton_Click error: {ex}");
            SetStatus("Unable to create a new meal product draft.");
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

            CurrentViewModel.AddMealItem(CurrentViewModel.SelectedMealForAdd);
            CurrentViewModel.StatusMessage = $"Added {CurrentViewModel.SelectedMealForAdd.MealName}.";
            CurrentViewModel.ValidateAll();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AddMealItemButton_Click error: {ex}");
            CurrentViewModel.StatusMessage = "Unable to add meal item.";
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
                CurrentViewModel.StatusMessage = "Meal item removed.";
                CurrentViewModel.ValidateAll();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"RemoveMealItemButton_Click error: {ex}");
            CurrentViewModel.StatusMessage = "Unable to remove meal item.";
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
                CurrentViewModel.StatusMessage = "Please fix validation errors before saving.";
                return;
            }

            var entity = CurrentViewModel.ToEntity();

            if (CurrentViewModel.MealProductID.HasValue)
            {
                Debug.WriteLine($"Updating meal product ID {CurrentViewModel.MealProductID.Value}");
                await _mealProductRepository.UpdateMealProductAsync(entity);
            }
            else
            {
                Debug.WriteLine("Creating new meal product");
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
                refreshed.StatusMessage = "Meal product saved successfully.";
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
            CurrentViewModel.StatusMessage = "Save failed. Check logs for details.";
        }
    }

    private async void DeleteMealProductButton_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentViewModel == null || !CurrentViewModel.MealProductID.HasValue)
        {
            SetStatus("Delete skipped. Select a saved meal product first.");
            return;
        }

        try
        {
            var id = CurrentViewModel.MealProductID.Value;
            await _mealProductRepository.DeleteMealProductAsync(id);

            await RefreshPageAsync();
            SetStatus("Meal product deleted successfully.");
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
}