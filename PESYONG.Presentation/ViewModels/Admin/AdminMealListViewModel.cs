using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.Domain.Entities.Meals.MealItem;

namespace PESYONG.Presentation.ViewModels.Admin;

public class AdminMealListViewModel : ObservableCollection<MealViewModel>
{
    public ObservableCollection<MealViewModel> Meals { get; } = new();
    // reference to the repository (or API if you have an actual backend)
    private readonly MealRepository _mealRepository;

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
        List<Meal> _dbMeals = await _mealRepository.GetAllMealsAsync();

        foreach (Meal meal in _dbMeals)
        {
            Meals.Add(new MealViewModel(meal, this));
        }
    }

    private void CreateMeal()
    {
        // Basically clear the VM, populate with data.

        // This is WRONG. It's two way bound with population. you can just fill it in, then feed it to the server then debounce it.


        //var emptyMeal = new Meal
        //{
        //    MealName = "",
        //    Description = ""
        //};

        //var newMealVm = new MealViewModel(emptyMeal, this);
        //Meals.Add(newMealVm);
        //SelectedMeal = newMealVm; 
    }

    public void SaveMeal()
    {
        Meal _newMeal = new Meal();
        Meals.Add(new MealViewModel(_newMeal, this));
    }

    public async Task DeleteMeal(MealViewModel mealVm)
    {
        // Technical debt, unhandled deletion, not logged
        if (mealVm.MealID != null)
        {
            Meals.Remove(mealVm);
            await _mealRepository.DeleteMealAsync((int)mealVm.MealID); 
        }
    }
}
