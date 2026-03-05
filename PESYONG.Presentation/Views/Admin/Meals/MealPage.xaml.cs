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
    private MealViewModel _newMeal;

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
    }

    private void UpdateBindings()
    {
        var currentMeal = _viewModel?.SelectedMeal ?? _newMeal;

        if (currentMeal != null)
        {
            StockStatusBorder.Background = new SolidColorBrush(
                currentMeal.IsAvailable ? Colors.Green : Colors.Red);

            SaveButton.Background = new SolidColorBrush(
                currentMeal.HasChanges ? Colors.Green : Colors.Gray);
            SaveButton.IsEnabled = currentMeal.HasChanges;

            if (!string.IsNullOrEmpty(currentMeal.ImageSourceString))
            {
                MealImageControl.Source = new BitmapImage(new Uri(currentMeal.ImageSourceString));
            }
        }
        else
        {
            StockStatusBorder.Background = new SolidColorBrush(Colors.Gray);
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
        var currentMeal = _viewModel?.SelectedMeal ?? _newMeal;

        if (currentMeal != null)
        {
            currentMeal.DeliveryType = (DeliveryType)comboBox.SelectedIndex;
            UpdateBindings(); 
        }
    }



    // Your existing methods integrated below:

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeExistingMealsAsync();
        UpdateBindings();
    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListView lv)
        {
            _viewModel.SelectedMeal = lv.SelectedItem as MealViewModel;
        }
    }
    

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var currentMeal = _viewModel?.SelectedMeal ?? _newMeal;

        if (currentMeal != null)
        {
            currentMeal.HasChanges = true;
            UpdateBindings();
        }
    }

    private void NumberBox_ValueChanged(object sender, NumberBoxValueChangedEventArgs e)
    {
        var currentMeal = _viewModel?.SelectedMeal ?? _newMeal;

        if (currentMeal != null)
        {
            currentMeal.HasChanges = true;
            UpdateBindings();
        }
    }

    // If you press the add new meal item button, then you switch out to the newMeal model.
    // Then, data is passed there. Then fed to its parent, which will return an acceptable viewmodel.
    private void AddMealButton_Clicked(object sender, RoutedEventArgs e)
    {
        _newMeal = new MealViewModel(_viewModel);
        _viewModel.SelectedMeal = null;
        MealFormsPanel.DataContext = _newMeal;
        UpdateBindings();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedMeal = _viewModel?.SelectedMeal;
        if (selectedMeal != null)
        {
            await _viewModel.DeleteMeal(selectedMeal);
            UpdateVisibility();
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Create a new item
        if (_viewModel?.SelectedMeal == null && _newMeal.MealID == null)
        {
            // Add validation here to complain if the datas aren't correct
            if (_viewModel != null)
            {
                await _viewModel.CreateMeal(_newMeal);
                UpdateBindings();
            } // handle exceptions here            
        }
        // Update existing item in the stack
        else if (_viewModel?.SelectedMeal != null)
        {
            _viewModel.UpdateSelectedMealCommand.Execute(_viewModel.SelectedMeal);
            UpdateBindings();
        }
    }

    private void ShowQueryPopupButton_Clicked(object sender, RoutedEventArgs e)
    {
        if (!QueryPopup.IsOpen) { QueryPopup.IsOpen = true; }
    }

    private void CloseQueryPopupButton_Clicked(object sender, RoutedEventArgs e)
    {
        if (QueryPopup.IsOpen) { QueryPopup.IsOpen = false; }
    }
}
