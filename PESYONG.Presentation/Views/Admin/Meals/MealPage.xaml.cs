using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
        Loaded += MealPage_Loaded;
    }

    public MealPage(AdminMealListViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        DataContext = _viewModel; // Set DataContext for binding

        if (_viewModel != null)
        {
            _ = _viewModel.InitializeExistingMealsAsync();
        }
    }

    private async void MealPage_Loaded(object sender, RoutedEventArgs e)
    {
        await InitializePageAsync();
    }

    private async Task InitializePageAsync()
    {
        UpdateVisibility();

        if (_viewModel?.Meals.Count == 0)
        {
            await _viewModel.InitializeExistingMealsAsync();
        }

        UpdateBindings();
    }


    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AdminMealListViewModel.SelectedMeal))
        {
            UpdateVisibility();
            UpdateBindings();
        }
    }

    private void UpdateVisibility()
    {
        var hasSelection = _viewModel?.SelectedMeal != null;
        MainPanel.Visibility = Visibility.Visible;
        //MainPanel.Visibility = hasSelection ? Visibility.Visible : Visibility.Collapsed;
        //NoSelectionMessage.Visibility = hasSelection ? Visibility.Collapsed : Visibility.Visible;
    }

    private void UpdateBindings()
    {
        var selectedMeal = _viewModel?.SelectedMeal;

        if (selectedMeal != null)
        {
            // Update stock status color
            StockStatusBorder.Background = new SolidColorBrush(
                selectedMeal.IsAvailable ? Colors.Green : Colors.Red);

            // Update low stock visibility
            LowStockWarning.Visibility = selectedMeal.StockQuantity <= 5 ?
                Visibility.Visible : Visibility.Collapsed;

            // Update save button state
            SaveButton.Background = new SolidColorBrush(
                selectedMeal.HasChanges ? Colors.Green : Colors.Gray);
            SaveButton.IsEnabled = selectedMeal.HasChanges;

            // Update image
            if (!string.IsNullOrEmpty(selectedMeal.ImageSourceString))
            {
                MealImageControl.Source = new BitmapImage(new Uri(selectedMeal.ImageSourceString));
            }
        }
        else
        {
            // Reset to defaults
            StockStatusBorder.Background = new SolidColorBrush(Colors.Gray);
            LowStockWarning.Visibility = Visibility.Collapsed;
            SaveButton.Background = new SolidColorBrush(Colors.Gray);
            SaveButton.IsEnabled = false;
            MealImageControl.Source = null;
        }
    }

    private void DeliveryTypeComboBox_Loaded(object sender, RoutedEventArgs e)
    {
        var comboBox = (ComboBox)sender;
        var selectedMeal = _viewModel?.SelectedMeal;

        if (selectedMeal != null)
        {
            comboBox.SelectedIndex = (int)selectedMeal.DeliveryType;
        }
    }

    private void DeliveryTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var comboBox = (ComboBox)sender;
        var selectedMeal = _viewModel?.SelectedMeal;

        if (selectedMeal != null)
        {
            selectedMeal.DeliveryType = (DeliveryType)comboBox.SelectedIndex;
            UpdateBindings(); // Refresh UI
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedMeal = _viewModel?.SelectedMeal;
        if (selectedMeal != null)
        {
            _viewModel.DeleteMeal(selectedMeal);
            UpdateVisibility(); // Refresh UI after deletion
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedMeal = _viewModel?.SelectedMeal;
        if (selectedMeal != null)
        {
            _viewModel.UpdateSelectedMealCommand.Execute(null);
            UpdateBindings(); // Refresh UI after save
        }
    }

    // Your existing methods integrated below:

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeExistingMealsAsync();
        UpdateBindings();
    }

    public void AddItems()
    {
        Meal sampleMeal1 = new Meal
        {
            MealID = 0,
            OperatorID = 123,
            MealName = "Spaghetti Carbonara",
            Description = "Classic Italian pasta with creamy sauce and bacon",
            MealPrice = 16.26m,
            StockQuantity = 3,
            MealTags = { MealTagType.Dietary },
            DeliveryType = DeliveryType.Express,
            ImageSourceString = "ms-appx:///Assets/SampleMeal",
            CreationDate = DateTime.Now,
        };

        Meal sampleMeal2 = new Meal
        {
            MealID = 3,
            OperatorID = 123,
            MealName = "Spaghetti Assini",
            Description = "Classic Italian pasta with creamy sauce and bacon",
            MealPrice = 16.26m,
            StockQuantity = 3,
            MealTags = { MealTagType.Dietary },
            DeliveryType = DeliveryType.Express,
            ImageSourceString = "ms-appx:///Assets/SampleMeal",
            CreationDate = DateTime.Now,
        };

        _viewModel.CreateMeal(new MealViewModel(sampleMeal1, _viewModel));
        _viewModel.CreateMeal(new MealViewModel(sampleMeal2, _viewModel));
        UpdateBindings(); // Refresh UI after adding items
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
            UpdateBindings(); // Refresh UI after update
        }
    }

    // New method to handle text changes for real-time updates
    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateBindings(); // Refresh UI when text changes
    }
}
