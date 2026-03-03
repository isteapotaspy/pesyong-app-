using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using PESYONG.ApplicationLogic.DTOs;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.ApplicationLogic.Utilities;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Enums;
using PESYONG.Presentation.ViewModels.Admin;
using Windows.Storage;

namespace PESYONG.Presentation.Views.Admin.Meals;

/// <summary>
/// This is the page for editing Meals in Admin, navigatable though AdminLayout.
/// </summary>

public sealed partial class MealPage : Page 
{
    private AdminMealListViewModel _viewModel;

    public MealPage()
    {
        InitializeComponent();
    }

    public MealPage(AdminMealListViewModel viewModel) : this()
    {
        _viewModel = viewModel;

        if (_viewModel != null)
        {
            _ = _viewModel.InitializeExistingMealsAsync();
        }
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeExistingMealsAsync();
    }

    public void AddItems()
    { 
        Meal sampleMeal1 = new Meal
        { 
            MealID = 0,
            OperatorID = 123,
            MealName = "Spaghetti Carbonara",
            Description = "Classic Italian pasta with creamy sauce and bacon",

        }
        _viewModel.SelectedMeal.Add
    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListView lv)
        {
            _viewModel.SelectedMeal = lv.SelectedItem as MealViewModel;
        }
    }

    private void UpdateMealItemButton_Clicked(object sender, RoutedEventArgs e)
    {
        if (_viewModel?.SelectedMeal != null)
        {
            _viewModel.UpdateSelectedMealCommand.Execute(_viewModel.SelectedMeal);
        }
    }
}