using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Data;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Enums;
using PESYONG.Presentation.Views.Admin.Meals;

namespace PESYONG.Presentation.ViewModels.Admin;

/// <remarks>
/// TASK: There's error _meals & Meals syncing error. 
/// DESCRIPTION: Meals itself, as well as the VM, Entity and Repostory is working.
/// But there's syncing issues between the meals.
/// </remarks>

public partial class AdminMealListViewModel : ObservableObject
{
    // Explicit observable property replacing the source-generator-backed field.
    private MealViewModel? _selectedMeal;
    private bool _IsValidEnteredData;
    public MealViewModel? SelectedMeal
    {
        get => _selectedMeal;
        set => SetProperty(ref _selectedMeal, value);
    }

    private ObservableCollection<MealViewModel> _meals = new();
    public ObservableCollection<MealViewModel> Meals
    {
        get => _meals;
        // Use the traditional MVVM methods
        // Replace this instead of SetProperty
        set => SetProperty(ref _meals, value);
    }

    private readonly MealRepository _mealRepository;
    public MealRepository MealRepository => _mealRepository;

    private readonly Meal sampleMeal1 = new Meal
    {
        OperatorID = 0,
        MealName = "Jobillat Jobisong",
        Description = "Find out the LGBT Food item with the juiciest pussy of them all.",
        MealPrice = 16.26m,
        StockQuantity = 90,
        MealTags = { "Halal" },
        DeliveryType = DeliveryType.Express,
        LastModifiedByOperatorID = 0,
        LastModifiedDate = DateTime.Now,
        ImageSourceString = "ms-appx:///Assets/SampleMeal.png",
    };

    private readonly Meal sampleMeal2 = new Meal
    {
        OperatorID = 0,
        MealName = "McDonaldo Bambino",
        Description = "Spaghettino faggetino, babino skibidi rizzler OH HELL NAWWWWW",
        MealPrice = 78.26m,
        StockQuantity = 3,
        MealTags = { "Kosher" },
        DeliveryType = DeliveryType.Express,
        ImageSourceString = "ms-appx:///Assets/SampleMeal.png",
    };

    public AdminMealListViewModel(MealRepository mealRepository)
    {
        Meals = new ObservableCollection<MealViewModel>();
        BindingOperations.EnableCollectionSynchronization(Meals, _lockObject);
        SelectedMeal = null;
        _mealRepository = mealRepository ?? throw new ArgumentNullException(nameof(mealRepository), "MealRepository must be registered in DI container");

        Debug.Write("\n\nThe first meal is " + sampleMeal1.IsValid() + "\n\n");
        MealViewModel mealvm1 = new MealViewModel(sampleMeal1, this);
        Debug.Write("\n\nThe first meal view model is " + mealvm1.IsValid + "\n\n");

        // Use other patterns for this later
        _ = Task.Run(async () =>
        {
            await CreateMeal(new MealViewModel(sampleMeal1, this));
            await CreateMeal(new MealViewModel(sampleMeal2, this));
            await InitializeExistingMealsAsync();
        });
    }

    public async Task InitializeExistingMealsAsync()
    {
        try
        {
            if (_mealRepository == null)
            {
                Debug.WriteLine("Error: MealRepository is null. Ensure it's registered in the DI container.");
                return;
            }

            var dbMeals = await _mealRepository.GetAllMealsAsync();

            if (dbMeals.Any())
            {
                foreach (var meal in dbMeals)
                {
                    Meals.Add(new MealViewModel(meal, this));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading meals: {ex.Message}");
        }
    }

    public async Task CreateMeal(MealViewModel newMeal)
    {
        try
        {
            Meal createdMeal = await _mealRepository.CreateMealAsyncReturnSelf(newMeal.GetMeal());
            MealViewModel createdVM = new MealViewModel(createdMeal, this);

            Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
            {

                Meals?.Add(createdVM);
                if (Meals?.Count > 0) SelectedMeal = createdVM;

                Debug.Write($"[DEBUG] Item added. New count: {Meals?.Count}");
                Debug.Write($"[DEBUG] Created item initialized in SelectedMeal.");
            });
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"[ERROR] CreatedMeal failed: {ex.Message}");
        }
        

    }

    [RelayCommand]
    public async Task UpdateSelectedMeal()
    {
        if (SelectedMeal?.HasChanges == true)
        {
            var mealToUpdate = SelectedMeal.GetMeal();
            await _mealRepository.UpdateMealAsync(mealToUpdate);
            SelectedMeal.HasChanges = false;
        }
    }

    public async Task DeleteMeal(MealViewModel mealVm)
    {
        if (mealVm.MealID != null)
        {
            Meals.Remove(mealVm);
            await _mealRepository.DeleteMealAsync((int)mealVm.MealID);
        }
    }
}
