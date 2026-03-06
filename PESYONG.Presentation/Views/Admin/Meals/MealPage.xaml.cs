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
using System.Threading.Tasks;

namespace PESYONG.Presentation.Views.Admin.Meals
{
    public sealed partial class MealPage : Page
    {
        private readonly MealRepository _mealRepository;

        public ObservableCollection<MealViewModel> MealListViewModels { get; } = new();

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

            foreach (Meal meal in GetSeedMeals())
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
                var mealViewModel = MealViewModel.CreateFromEntity(meal, _mealRepository);
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
                var emptyVm = new MealViewModel(_mealRepository);
                emptyVm.ClearMealViewModel();
                emptyVm.MinOrderQuantity = 1;
                emptyVm.DeliveryType = DeliveryType.Delivery;

                DataContext = emptyVm;
            }
        }

        private void AddMealButton_Click(object sender, RoutedEventArgs e)
        {
            var newMealVm = new MealViewModel(_mealRepository);
            newMealVm.ClearMealViewModel();
            newMealVm.MinOrderQuantity = 1;
            newMealVm.StockQuantity = 0;
            newMealVm.DeliveryType = DeliveryType.Delivery;
            newMealVm.CreationDate = DateTime.UtcNow;
            newMealVm.LastModifiedDate = DateTime.UtcNow;

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

                if (vm.MealID.HasValue)
                {
                    Debug.WriteLine($"Updating meal ID {vm.MealID.Value}...");
                    await _mealRepository.UpdateMealAsync(vm.ToEntity());
                }
                else
                {
                    Debug.WriteLine("Creating new meal...");
                    var createdMeal = await _mealRepository.CreateMealAsyncReturnSelf(vm.ToEntity());
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

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListView)sender).SelectedItem is MealViewModel selectedMeal)
            {
                DataContext = selectedMeal;
                UpdatePageForm();
            }
        }

        private void UpdatePageForm()
        {
            // keep empty for now unless you want extra visual logic
        }

        private List<Meal> GetSeedMeals()
        {
            return new List<Meal>
            {
                new Meal
                {
                    MealName = "Puto",
                    MealPrice = 60,
                    Description = "Soft and fluffy steamed rice cake",
                    ImageSourceString = "ms-appx:///Assets/Images/puto.jpg",
                    StockQuantity = 50,
                    MinOrderQuantity = 6,
                    DeliveryType = DeliveryType.Delivery,
                    MealTags = new List<string> { "Makakalibanga", "Makapapurigit" }
                },
                new Meal
                {
                    MealName = "Kutsinta",
                    MealPrice = 50,
                    Description = "Brown rice cake with coconut topping",
                    ImageSourceString = "ms-appx:///Assets/Images/kutsinta.jpg",
                    StockQuantity = 45,
                    MinOrderQuantity = 6,
                    DeliveryType = DeliveryType.Delivery,
                    MealTags = new List<string> { "Makakalibanga", "Makapapurigit" }
                },
                new Meal
                {
                    MealName = "Bibingka",
                    MealPrice = 80,
                    Description = "Traditional baked rice cake",
                    ImageSourceString = "ms-appx:///Assets/Images/bibingka.jpg",
                    StockQuantity = 30,
                    MinOrderQuantity = 1,
                    DeliveryType = DeliveryType.Delivery,
                    MealTags = new List<string> { "Makakalibanga", "Makapapurigit" }
                },
                new Meal
                {
                    MealName = "Suman",
                    MealPrice = 70,
                    Description = "Sticky rice wrapped in banana leaves",
                    ImageSourceString = "ms-appx:///Assets/Images/suman.jpg",
                    StockQuantity = 40,
                    MinOrderQuantity = 6,
                    DeliveryType = DeliveryType.Delivery,
                    MealTags = new List<string> { "Makakalibanga", "Makapapurigit" }
                },
                new Meal
                {
                    MealName = "Sapin-Sapin",
                    MealPrice = 90,
                    Description = "Multi-layered sweet rice cake",
                    ImageSourceString = "ms-appx:///Assets/Images/sapin-sapin.jpg",
                    StockQuantity = 25,
                    MinOrderQuantity = 1,
                    DeliveryType = DeliveryType.Delivery,
                    MealTags = new List<string> { "Makakalibanga", "Makapapurigit" }
                },
                new Meal
                {
                    MealName = "Biko",
                    MealPrice = 75,
                    Description = "Sweet sticky rice with coconut caramel",
                    ImageSourceString = "ms-appx:///Assets/Images/biko.jpg",
                    StockQuantity = 35,
                    MinOrderQuantity = 1,
                    DeliveryType = DeliveryType.Delivery,
                    MealTags = new List<string> { "Makakalibanga", "Makapapurigit" }
                }
            };
        }
    }
}