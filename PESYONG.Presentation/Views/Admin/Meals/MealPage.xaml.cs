using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using PESYONG.ApplicationLogic.DTOs;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.ApplicationLogic.Utilities;
using PESYONG.ApplicationLogic.ViewModels.ObjectModels;
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
    private readonly MealRepository MealRepository;
    private MealViewModel SelectedMealViewModel;
    private ObservableCollection<MealViewModel> MealListViewModels;

    public MealPage()
    {
        InitializeComponent();
    }

    public MealPage(MealViewModel MealViewModel) : this()
    {
        SelectedMealViewModel = MealViewModel;
        SelectedMealViewModel.ClearMealViewModel();
        DataContext = MealViewModel;

        this.Loaded += async (s, e) => await InitializeMealListViewModels();
    }

    private async Task InitializeMealListViewModels()
    {
        var mealList = await MealRepository.GetAllMealsAsync();

        MealListViewModels = new ObservableCollection<MealViewModel>(
            mealList.Select(meal => MealViewModel.CreateFromEntity(meal, MealRepository))
        );
    }

    private void AddMealButton_Click(object sender, RoutedEventArgs e) { }

    private void DeleteButton_Click(object sender, RoutedEventArgs e) { }

    private void SaveButton_Click(object sender, RoutedEventArgs e) { }

    private void ShowQueryPopupButton_Click(object sender, RoutedEventArgs e)
    {
        if (!QueryPopup.IsOpen) { QueryPopup.IsOpen = true; }
    }

    private void CloseQueryPopupButton_Click(object sender, RoutedEventArgs e)
    {
        if (QueryPopup.IsOpen) { QueryPopup.IsOpen = false; }
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Your code here
    }

    private void NumberBox_ValueChanged(object sender, NumberBoxValueChangedEventArgs e) { }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var listView = (ListView)sender;
        SelectedMealViewModel = (MealViewModel)listView.SelectedItem;

        Debug.WriteLine($"\n\nSelected: {SelectedMealViewModel?.MealName}\n");
    }
}
