using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using PESYONG.ApplicationLogic.DTOs.Meals.Meal;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Presentation.ViewModels.Admin;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PESYONG.Presentation.Views.Admin.Meals;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MealPage : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    public ObservableCollection<MealViewModel> Meals { get; } = new ObservableCollection<MealViewModel>();

    public MealPage()
    {
        InitializeComponent();
        LoadFakeMeals(); 
    }

    private MealViewModel _selectedMeal;
    public MealViewModel SelectedMeal
    {
        get => _selectedMeal;
        set
        {
            _selectedMeal = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedMeal)));
        }
    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedMeal = (MealViewModel)((ListView)sender).SelectedItem;
    }

    private void LoadFakeMeals()
    {
        Meals.Clear();
        var fakeMeals = new List<MealViewModel>
        {
            new MealViewModel
            {
                MealID = 1,
                MealName = "Grilled Chicken Salad",
                Description = "Fresh salad with grilled chicken, mixed greens, and balsamic dressing",
                MealPrice = 12.99m,
                StockQuantity = 8,
                MinOrderQuantity = 1,
                Tags = new List<string> { "Healthy", "Low-Carb", "Gluten-Free" },
                DeliveryType = "Express",
                ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/SampleMeal.png"))
            },
            new MealViewModel
            {
                MealID = 2,
                MealName = "Beef Burger Combo",
                Description = "Juicy beef burger with fries and drink",
                MealPrice = 15.50m,
                StockQuantity = 3,
                MinOrderQuantity = 1,
                Tags = new List<string> { "Combo", "Popular", "American" },
                DeliveryType = "Standard",
                ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/SampleMeal.png"))
            },
            new MealViewModel
            {
                MealID = 3,
                MealName = "Vegetarian Pasta",
                Description = "Creamy pasta with fresh vegetables and herbs",
                MealPrice = 10.75m,
                StockQuantity = 15,
                MinOrderQuantity = 1,
                Tags = new List<string> { "Vegetarian", "Italian", "Comfort Food" },
                DeliveryType = "Express",
                ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/SampleMeal.png"))
            },
            new MealViewModel
            {
                MealID = 4,
                MealName = "Sushi Platter",
                Description = "Assorted sushi rolls with soy sauce and wasabi",
                MealPrice = 22.99m,
                StockQuantity = 0, // Out of stock
                MinOrderQuantity = 1,
                Tags = new List<string> { "Japanese", "Fresh", "Premium" },
                DeliveryType = "Express",
                ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/SampleMeal.png"))
            }
        };

        foreach (var meal in fakeMeals)
        {
            Meals.Add(meal);
        }
    }
}