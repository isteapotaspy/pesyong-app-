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
using Microsoft.Windows.Storage.Pickers;
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
    private MealViewModel MealViewModel;
    private ObservableCollection<MealViewModel> MealListViewModels = new();

    public MealPage()
    {
        InitializeComponent();
    }

    public MealPage(MealViewModel MealViewModel, MealRepository MealRepository) : this()
    {
        this.MealRepository = MealRepository;
        this.MealViewModel = MealViewModel;
        DataContext = MealViewModel;

        this.Loaded += async (s, e) => await InitializeMealListViewModels();
    }

    private async Task InitializeMealListViewModels()
    {
        foreach (Meal meal in meals)
        {
            Debug.WriteLine($"\n\nMealPage.xaml.cs tried to add meal {meal.MealName} in the database");
            await MealRepository.CreateMealAsync(meal);
        }

        var allMeals = await MealRepository.GetAllMealsAsync();
        Debug.WriteLine($"Total meals in database: {allMeals.Count}");
        
        foreach (var meal in allMeals)
        {
            Debug.WriteLine($"Meal ID: {meal.MealID}, Name: {meal.MealName}");

            var mealViewModel = MealViewModel.CreateFromEntity(meal, MealRepository);
            MealListViewModels.Add(mealViewModel);
        }
    }

    private void AddMealButton_Click(object sender, RoutedEventArgs e)
    {
        this.MealViewModel = new MealViewModel(MealRepository);
        DataContext = MealViewModel;
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.MealViewModel?.MealID != null)
        {
            this.MealViewModel.DeleteCommand.Execute(null);
            MealListViewModels.Remove(this.MealViewModel);
        }

        this.MealViewModel = new MealViewModel(MealRepository);
        DataContext = MealViewModel;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.MealViewModel.HasValidationErrors)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            dialog.Title ="There are errors ";
            dialog.PrimaryButtonText = "Keep editing";
            dialog.SecondaryButtonText = "Clear form";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.None;

            var result = await dialog.ShowAsync();
        }
        else if (this.MealViewModel.MealID == null)
        {
            Meal Self = await MealRepository.CreateMealAsyncReturnSelf(this.MealViewModel.ToEntity());
            this.MealViewModel.LoadFromEntity(Self);
            MealListViewModels.Add(this.MealViewModel);
        }
        else
        {
            this.MealViewModel.SaveCommand.Execute(null);

            int index = MealListViewModels.IndexOf(this.MealViewModel);
            if (index >= 0)
            {
                MealListViewModels[index] = this.MealViewModel;
            }
        }
    }

    private async void ChangePhoto_Click(object sender, RoutedEventArgs e) 
    {
        if (sender is Button button)
        {
            button.IsEnabled = false;

            var picker = new FileOpenPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId);
            picker.CommitButtonText = "Pick File";
                        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.ViewMode = PickerViewMode.List;

            var file = await picker.PickSingleFileAsync();
            //PickedSingleFileTextBlock.Text = file != null
            //    ? "Picked: " + file.Path
            //    : "No file selected.";

            //re-enable the button
            button.IsEnabled = true;
        }

    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var listView = (ListView)sender;
        if (listView.SelectedItem is MealViewModel selectedMeal)
        {
            this.MealViewModel = selectedMeal;
            DataContext = MealViewModel;
        }
    }

    private void ShowQueryPopupButton_Click(object sender, RoutedEventArgs e)
    {
        if (!QueryPopup.IsOpen) { QueryPopup.IsOpen = true; }
    }

    private void CloseQueryPopupButton_Click(object sender, RoutedEventArgs e)
    {
        if (QueryPopup.IsOpen) { QueryPopup.IsOpen = false; }
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e) { }

    private void NumberBox_ValueChanged(object sender, NumberBoxValueChangedEventArgs e) { }

    List<Meal> meals = new List<Meal>
            {
                new Meal
                {
                    MealID = 1,
                    MealName = "Puto",
                    MealPrice = 60,
                    Description = "Soft and fluffy steamed rice cake",
                    ImageSourceString = "ms-appx:///Assets/Images/puto.jpg",
                    StockQuantity = 50,
                    MinOrderQuantity = 6, // Sold by dozens
                    MealTags = new List<String> { "Halal", "Kosher" }
                },
                new Meal
                {
                    MealName = "Kutsinta",
                    MealPrice = 50,
                    Description = "Brown rice cake with coconut topping",
                    ImageSourceString = "ms-appx:///Assets/Images/kutsinta.jpg",
                    StockQuantity = 45,
                    MinOrderQuantity = 6,
                    MealTags = new List<String> { "Halal", "Kosher" }
                },
                new Meal
                {
                    MealID = 3,
                    MealName = "Bibingka",
                    MealPrice = 80,
                    Description = "Traditional baked rice cake",
                    ImageSourceString = "ms-appx:///Assets/Images/bibingka.jpg",
                    StockQuantity = 30,
                    MinOrderQuantity = 1,
                    MealTags = new List<String> { "Halal", "Kosher" }
                },
                new Meal
                {
                    MealID = 4,
                    MealName = "Suman",
                    MealPrice = 70,
                    Description = "Sticky rice wrapped in banana leaves",
                    ImageSourceString = "ms-appx:///Assets/Images/suman.jpg",
                    StockQuantity = 40,
                    MinOrderQuantity = 6,
                    MealTags = new List<String> { "Halal", "Kosher" }
                },
                new Meal
                {
                    MealID = 5,
                    MealName = "Sapin-Sapin",
                    MealPrice = 90,
                    Description = "Multi-layered sweet rice cake",
                    ImageSourceString = "ms-appx:///Assets/Images/sapin-sapin.jpg",
                    StockQuantity = 25,
                    MinOrderQuantity = 1,
                    MealTags = new List<String> { "Halal", "Kosher" }
                },
                new Meal
                {
                    MealID = 6,
                    MealName = "Biko",
                    MealPrice = 75,
                    Description = "Sweet sticky rice with coconut caramel",
                    ImageSourceString = "ms-appx:///Assets/Images/biko.jpg",
                    StockQuantity = 35,
                    MinOrderQuantity = 1,
                    MealTags = new List<String> { "Halal", "Kosher" }
                }
            };
}
