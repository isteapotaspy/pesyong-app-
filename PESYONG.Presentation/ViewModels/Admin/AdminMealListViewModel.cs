using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Presentation.Views.Admin.Meals;

namespace PESYONG.Presentation.ViewModels.Admin;

public partial class AdminMealListViewModel : ObservableCollection<MealViewModel>, INotifyPropertyChanged
{
    public ObservableCollection<MealViewModel> Meals { get; } = new();
    private readonly MealRepository _mealRepository;
    public MealRepository MealRepository => _mealRepository;

    public AdminMealListViewModel(MealRepository mealRepository)
    {
        _mealRepository = mealRepository ?? throw new ArgumentNullException(nameof(mealRepository), "MealRepository must be registered in DI container");
    }

    private MealViewModel? _selectedMeal;
    public MealViewModel? SelectedMeal
    {
        get => _selectedMeal;
        set
        {
            if (!EqualityComparer<MealViewModel?>.Default.Equals(_selectedMeal, value))
            {
                _selectedMeal = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedMeal)));
            }
        }
    }

    public async Task InitializeExistingMealsAsync()
    {
        try
        {
            if (_mealRepository == null)
            {
                Console.WriteLine("Error: MealRepository is null. Ensure it's registered in the DI container.");
                return;
            }

            List<Meal> _dbMeals = await _mealRepository.GetAllMealsAsync();

            if (_dbMeals.Any())
            {
                foreach (Meal meal in _dbMeals)
                {
                    Meals.Add(new MealViewModel(meal, this));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading meals: {ex.Message}");
        }
    }

    private void CreateMeal(MealViewModel MealVM)
    {
        Meals.Add(MealVM);
        SelectedMeal = MealVM;
    }

    [RelayCommand]
    private async Task UpdateSelectedMeal()
    {
        if (SelectedMeal?.HasChanges == true)
        {
            Meal mealToUpdate = SelectedMeal.GetMeal();
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
