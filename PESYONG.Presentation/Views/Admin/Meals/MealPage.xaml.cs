using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
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
using Windows.Storage.Streams;
using WinRT.Interop;

namespace PESYONG.Presentation.Views.Admin.Meals
{
    public sealed partial class MealPage : Page
    {
        private readonly MealSyncService _mealSyncService;
        private readonly MealRepository _mealRepository;

        public ObservableCollection<MealViewModel> MealListViewModels { get; } = new();
        private MealViewModel? SelectedMealViewModel => DataContext as MealViewModel;

        public Array DeliveryTypes { get; } = Enum.GetValues(typeof(DeliveryType));

        private MealViewModel? SelectedMealViewModel => DataContext as MealViewModel;

        private readonly MealSyncService _mealSyncService;
        public MealPage()
        {
            this.InitializeComponent();

            _mealRepository = App.Current.Services.GetRequiredService<MealRepository>();
            _mealSyncService = App.Current.Services.GetRequiredService<MealSyncService>();

            this.Loaded += MealPage_Loaded;
        }

        private async void MealPage_Loaded(object sender, RoutedEventArgs e)
        {
            await EnsureSeedDataAsync();
            await RefreshMealListAsync();
        }

        private async Task EnsureSeedDataAsync()
        {
            var existingMeals = await _mealRepository.GetAllMealsAsync();

            if (existingMeals.Any())
            {
                Debug.WriteLine("Meals already exist in database. Skipping seed.");
                return;
            }

            Debug.WriteLine("Database is empty. Seeding starter meals...");

            var meals = await GetSeedMealsAsync();

            foreach (Meal meal in meals)
            {
                await _mealRepository.CreateMealAsync(meal);
            }
        }

        private async Task RefreshMealListAsync()
        {
            MealListViewModels.Clear();

            var allMeals = await _mealRepository.GetAllMealsAsync();
            Debug.WriteLine($"Loaded {allMeals.Count} meals from database.");

            foreach (var meal in allMeals.OrderBy(m => m.MealID))
            {
                var mealViewModel = MealViewModel.CreateFromEntity(meal);
                MealListViewModels.Add(mealViewModel);
            }

            if (MealListViewModels.Count > 0)
            {
                if (SelectedMealViewModel?.MealID is int selectedId)
                {
                    var matched = MealListViewModels.FirstOrDefault(x => x.MealID == selectedId);
                    if (matched != null)
                    {
                        MealsListView.SelectedItem = matched;
                        DataContext = matched;
                        return;
                    }
                }

                MealsListView.SelectedItem = MealListViewModels[0];
                DataContext = MealListViewModels[0];
            }
            else
            {
                var emptyVm = new MealViewModel();
                emptyVm.ClearMealViewModel();
                emptyVm.MinOrderQuantity = 1;
                emptyVm.DeliveryType = DeliveryType.Delivery;

                DataContext = emptyVm;
            }
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
                }

                _mealSyncService.NotifyMealsChanged();

                Debug.WriteLine("Meal saved successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Save failed: {ex.Message}");
            }
        }

        private async void ChangePhoto_Click(object sender, RoutedEventArgs e)
        { }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MealViewModel vm || !vm.MealID.HasValue)
            {
                Debug.WriteLine("Delete skipped: no saved meal selected.");
                return;
            }

            try
            {
                Debug.WriteLine($"Deleting meal ID {vm.MealID.Value}...");
                await _mealRepository.DeleteMealAsync(vm.MealID.Value);

                await RefreshMealListAsync();

                _mealSyncService.NotifyMealsChanged();


                Debug.WriteLine("Meal deleted successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Delete failed: {ex.Message}");
            }
        }

        private void ShowQueryPopupButton_Click(object sender, RoutedEventArgs e)
        {
            if (!QueryPopup.IsOpen)
            {
                QueryPopup.IsOpen = true;
            }
        }

        private void CloseQueryPopupButton_Click(object sender, RoutedEventArgs e)
        {
            if (QueryPopup.IsOpen)
            {
                QueryPopup.IsOpen = false;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // optional live UI updates
        }

        private void NumberBox_ValueChanged(object sender, NumberBoxValueChangedEventArgs e)
        {
            // optional live UI updates
        }

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListView)sender).SelectedItem is MealViewModel selectedMeal)
            {
                DataContext = selectedMeal;

                if (selectedMeal.MealID.HasValue)
                {
                    var fullMeal = await _mealRepository.GetMealByIdAsync(selectedMeal.MealID.Value);
                    _selectedImageBytes = fullMeal?.ImageBytes;
                }
                else
                {
                    _selectedImageBytes = null;
                }

                UpdatePageForm();
            }
        }

        private void UpdatePageForm()
        {
            // keep empty for now unless you want extra visual logic
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
                }

                Debug.WriteLine("Image selected successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Image pick failed: {ex.Message}");
            }
        }

        private async Task PickImageAsync()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            StorageFile file = await picker.PickSingleFileAsync();
            if (file == null) return;

            using var stream = await file.OpenReadAsync();
            _selectedImageBytes = new byte[stream.Size];
            await stream.ReadAsync(_selectedImageBytes.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
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
            catch
            {
                return null;
            }
        }
    }
}