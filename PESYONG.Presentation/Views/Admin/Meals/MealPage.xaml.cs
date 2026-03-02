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
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PESYONG.Presentation.Views.Admin.Meals;

/// <summary>
/// This is the page for editing Meals in Admin, navigatable though AdminLayout.
/// </summary>

public sealed partial class MealPage : Page 
{
    private event PropertyChangedEventHandler PropertyChanged;
    private readonly AdminMealListViewModel _viewModel = new AdminMealListViewModel();

    public MealPage()
    {
        InitializeComponent();
        DataContext = _viewModel;

        Loaded += async (s, e) => await _viewModel.InitializeExistingMealsAsync();
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
            _viewModel.SaveMeal();
        }
    }
}