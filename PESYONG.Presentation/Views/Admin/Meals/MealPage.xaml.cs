using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.ApplicationLogic.Services;
using PESYONG.ApplicationLogic.ViewModels.ObjectModels;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using WinRT.Interop;

namespace PESYONG.Presentation.Views.Admin.Meals
{
    public sealed partial class MealPage : Page
    {
        private readonly MealSyncService _mealSyncService;
        private readonly MealRepository _mealRepository;
        private byte[]? _selectedImageBytes;
        private bool _isLoading;

        public ObservableCollection<MealViewModel> MealListViewModels { get; } = new();

        private MealViewModel? SelectedMealViewModel => DataContext as MealViewModel;

        public Array DeliveryTypes { get; } = Enum.GetValues(typeof(DeliveryType));

        public MealPage()
        {
            InitializeComponent();

            try
            {
                _mealRepository = App.Current.Services.GetRequiredService<MealRepository>();
                _mealSyncService = App.Current.Services.GetRequiredService<MealSyncService>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Service resolution failed: {ex}");
                throw;
            }

            Loaded += MealPage_Loaded;
        }

        private async void MealPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoading)
                return;

            try
            {
                _isLoading = true;

                await EnsureSeedDataAsync();
                await RefreshMealListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Page load failed: {ex}");
                SetStatus("Unable to load meals. Check logs for details.");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task EnsureSeedDataAsync()
        {
            try
            {
                var existingMeals = await _mealRepository.GetAllMealsAsync();

                if (existingMeals.Any())
                {
                    Debug.WriteLine("Meals already exist in database. Skipping seed.");
                    return;
                }

                Debug.WriteLine("Database is empty. Seeding starter meals...");

                var meals = await GetSeedMealsAsync();
                foreach (var meal in meals)
                {
                    await _mealRepository.CreateMealAsync(meal);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Seed failed: {ex}");
                throw;
            }
        }

        private async Task RefreshMealListAsync()
        {
            try
            {
                var selectedId = SelectedMealViewModel?.MealID;

                MealListViewModels.Clear();

            foreach (var meal in allMeals.OrderBy(m => m.MealID))
            {
                var mealViewModel = MealViewModel.CreateFromEntity(meal);
                MealListViewModels.Add(mealViewModel);
            }

                foreach (var meal in allMeals.OrderBy(m => m.MealID))
                {
                    MealListViewModels.Add(MealViewModel.CreateFromEntity(meal));
                }

                if (MealListViewModels.Count == 0)
                {
                    var emptyVm = CreateDefaultMealViewModel();
                    emptyVm.StatusMessage = "No meals found. Create a new one.";
                    DataContext = emptyVm;
                    MealsListView.SelectedItem = null;
                    _selectedImageBytes = null;
                    return;
                }

                var selectedVm = selectedId.HasValue
                    ? MealListViewModels.FirstOrDefault(x => x.MealID == selectedId.Value)
                    : MealListViewModels.FirstOrDefault();

                selectedVm ??= MealListViewModels.First();

                MealsListView.SelectedItem = selectedVm;
                DataContext = selectedVm;

                if (selectedVm.MealID.HasValue)
                {
                    var fullMeal = await _mealRepository.GetMealByIdAsync(selectedVm.MealID.Value);
                    _selectedImageBytes = fullMeal?.ImageBytes;
                }
                else
                {
                    _selectedImageBytes = null;
                }

                selectedVm.StatusMessage = string.Empty;
                selectedVm.RefreshComputedState();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Refresh failed: {ex}");
                SetStatus("Unable to refresh the meal list.");
            }
        }

        private MealViewModel CreateDefaultMealViewModel()
        {
            var vm = new MealViewModel();
            vm.ClearMealViewModel();
            vm.MinOrderQuantity = 1;
            vm.StockQuantity = 0;
            vm.DeliveryType = DeliveryType.Delivery;
            vm.CreationDate = DateTime.UtcNow;
            vm.LastModifiedDate = DateTime.UtcNow;
            vm.OperatorID = null;
            vm.LastModifiedByOperatorID = null;
            return vm;
        }

        private void AddMealButton_Click(object sender, RoutedEventArgs e)
        {
            var newMealVm = new MealViewModel();
            newMealVm.ClearMealViewModel();
            newMealVm.MinOrderQuantity = 1;
            newMealVm.StockQuantity = 0;
            newMealVm.DeliveryType = DeliveryType.Delivery;
            newMealVm.CreationDate = DateTime.UtcNow;
            newMealVm.LastModifiedDate = DateTime.UtcNow;
            newMealVm.OperatorID = null;
            newMealVm.LastModifiedByOperatorID = null;

            _selectedImageBytes = null;

            DataContext = newMealVm;
            MealsListView.SelectedItem = null;

            Debug.WriteLine("Created new draft meal.");
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MealViewModel vm)
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

                vm.LastModifiedDate = DateTime.UtcNow;

                var entity = vm.ToEntity();

                if (_selectedImageBytes != null)
                {
                    entity.ImageBytes = _selectedImageBytes;
                }
                else if (vm.MealID.HasValue)
                {
                    var existingMeal = await _mealRepository.GetMealByIdAsync(vm.MealID.Value);
                    if (existingMeal != null)
                    {
                        entity.ImageBytes = existingMeal.ImageBytes;
                    }
                }

                if (vm.MealID.HasValue)
                {
                    Debug.WriteLine($"Updating meal ID {vm.MealID.Value}...");
                    await _mealRepository.UpdateMealAsync(entity);
                }
                else
                {
                    Debug.WriteLine("Creating new meal...");
                    var createdMeal = await _mealRepository.CreateMealAsyncReturnSelf(entity);
                    vm.LoadFromEntity(createdMeal);
                }

                await RefreshMealListAsync();

                var refreshedVm = MealListViewModels.FirstOrDefault(x => x.MealID == vm.MealID);
                if (refreshedVm != null)
                {
                    MealsListView.SelectedItem = refreshedVm;
                    DataContext = refreshedVm;
                    refreshedVm.StatusMessage = "Meal saved successfully.";
                }

                _mealSyncService.NotifyMealsChanged();
                Debug.WriteLine("Meal saved successfully.");
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
            if (DataContext is not MealViewModel vm || !vm.MealID.HasValue)
            {
                Debug.WriteLine("Delete skipped: no saved meal selected.");
                SetStatus("Delete skipped. Select a saved meal first.");
                return;
            }

            try
            {
                Debug.WriteLine($"Deleting meal ID {vm.MealID.Value}...");
                await _mealRepository.DeleteMealAsync(vm.MealID.Value);

                await RefreshMealListAsync();

                _mealSyncService.NotifyMealsChanged();
                SetStatus("Meal deleted successfully.");

                Debug.WriteLine("Meal deleted successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Delete failed: {ex}");
                vm.StatusMessage = "Delete failed. Check logs for details.";
            }
        }

        private void ShowQueryPopupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!QueryPopup.IsOpen)
                {
                    QueryPopup.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Popup open failed: {ex}");
                SetStatus("Unable to open the filter popup.");
            }
        }

        private void CloseQueryPopupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (QueryPopup.IsOpen)
                {
                    QueryPopup.IsOpen = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Popup close failed: {ex}");
                SetStatus("Unable to close the filter popup.");
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is MealViewModel vm)
            {
                vm.StatusMessage = string.Empty;
                vm.ValidateAll();
                vm.RefreshComputedState();
            }
        }

        private void NumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (DataContext is MealViewModel vm)
            {
                vm.StatusMessage = string.Empty;
                vm.ValidateAll();
                vm.RefreshComputedState();
            }
        }

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoading)
                return;

            if (sender is not ListView listView)
                return;

            if (listView.SelectedItem is not MealViewModel selectedMeal)
                return;

            try
            {
                DataContext = selectedMeal;

                if (selectedMeal.MealID.HasValue)
                {
                    var fullMeal = await _mealRepository.GetMealByIdAsync(selectedMeal.MealID.Value);
                    _selectedImageBytes = fullMeal?.ImageBytes;
                    selectedMeal.ImageBytes = fullMeal?.ImageBytes;
                }
                else
                {
                    _selectedImageBytes = null;
                }

                UpdatePageForm();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Selection change failed: {ex}");
                selectedMeal.StatusMessage = "Unable to load the selected meal details.";
            }
        }

        private void UpdatePageForm()
        {
            if (DataContext is MealViewModel vm)
            {
                vm.RefreshComputedState();
            }
        }

        private async Task<List<Meal>> GetSeedMealsAsync()
        {
            return new List<Meal>
            {
                new Meal
                {
                    MealName = "Puto",
                    MealPrice = 60,
                    Description = "Soft and fluffy steamed rice cake",
                    ImageBytes = await LoadImageBytesAsync("Assets/SampleMeal.png"),
                    StockQuantity = 50,
                    MinOrderQuantity = 6,
                    DeliveryType = DeliveryType.Delivery,
                    MealTags = new List<string> { "Makakalibanga", "Makapapurigit" },
                    OperatorID = null,
                    LastModifiedByOperatorID = null
                },
                new Meal
                {
                    MealName = "Kutsinta",
                    MealPrice = 50,
                    Description = "Brown rice cake with coconut topping",
                    ImageBytes = await LoadImageBytesAsync("Assets/SampleMeal.png"),
                    StockQuantity = 45,
                    MinOrderQuantity = 6,
                    DeliveryType = DeliveryType.Delivery,
                    MealTags = new List<string> { "Makakalibanga", "Makapapurigit" },
                    OperatorID = null,
                    LastModifiedByOperatorID = null
                },
                new Meal
                {
                    MealName = "Bibingka",
                    MealPrice = 80,
                    Description = "Traditional baked rice cake",
                    ImageBytes = await LoadImageBytesAsync("Assets/SampleMeal.png"),
                    StockQuantity = 30,
                    MinOrderQuantity = 1,
                    DeliveryType = DeliveryType.Delivery,
                    MealTags = new List<string> { "Makakalibanga", "Makapapurigit" },
                    OperatorID = null,
                    LastModifiedByOperatorID = null
                },
                new Meal
                {
                    MealName = "Suman",
                    MealPrice = 70,
                    Description = "Sticky rice wrapped in banana leaves",
                    ImageBytes = await LoadImageBytesAsync("Assets/SampleMeal.png"),
                    StockQuantity = 40,
                    MinOrderQuantity = 6,
                    DeliveryType = DeliveryType.Delivery,
                    MealTags = new List<string> { "Makakalibanga", "Makapapurigit" },
                    OperatorID = null,
                    LastModifiedByOperatorID = null
                },
                new Meal
                {
                    MealName = "Sapin-Sapin",
                    MealPrice = 90,
                    Description = "Multi-layered sweet rice cake",
                    ImageBytes = await LoadImageBytesAsync("Assets/SampleMeal.png"),
                    StockQuantity = 25,
                    MinOrderQuantity = 1,
                    DeliveryType = DeliveryType.Delivery,
                    MealTags = new List<string> { "Makakalibanga", "Makapapurigit" },
                    OperatorID = null,
                    LastModifiedByOperatorID = null
                },
                new Meal
                {
                    MealName = "Biko",
                    MealPrice = 75,
                    Description = "Sweet sticky rice with coconut caramel",
                    ImageBytes = await LoadImageBytesAsync("Assets/SampleMeal.png"),
                    StockQuantity = 35,
                    MinOrderQuantity = 1,
                    DeliveryType = DeliveryType.Delivery,
                    MealTags = new List<string> { "Makakalibanga", "Makapapurigit" },
                    OperatorID = null,
                    LastModifiedByOperatorID = null
                }
            };
        }

        private async void PickImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await PickImageAsync();

                if (DataContext is MealViewModel vm)
                {
                    vm.ImageBytes = _selectedImageBytes;
                    vm.StatusMessage = _selectedImageBytes != null
                        ? "Image selected successfully."
                        : string.Empty;
                }

                Debug.WriteLine("Image selection flow completed.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Image pick failed: {ex}");
                SetStatus("Unable to select image.");
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
            {
                return;
            }

            using var stream = await file.OpenReadAsync();
            _selectedImageBytes = new byte[stream.Size];
            await stream.ReadAsync(_selectedImageBytes.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
        }

        private async Task<byte[]?> LoadImageBytesAsync(string relativePath)
        {
            try
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(
                    new Uri($"ms-appx:///{relativePath}"));

                using IRandomAccessStream stream = await file.OpenReadAsync();
                var bytes = new byte[stream.Size];
                await stream.ReadAsync(bytes.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);

                return bytes;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Asset image load failed: {ex}");
                return null;
            }
        }

        private void SetStatus(string message)
        {
            if (DataContext is MealViewModel vm)
            {
                vm.StatusMessage = message;
            }
        }
    }
}