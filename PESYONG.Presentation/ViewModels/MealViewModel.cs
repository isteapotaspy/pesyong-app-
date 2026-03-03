using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.Domain.Entities.Financial;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Users.Identity;
using PESYONG.Domain.Enums;

namespace PESYONG.Presentation.ViewModels.Admin;

/// <summary>
/// This handles the UI - Server data transfer for Meals.
/// This handles UI actions, coordinates model updates and
/// server communication, as well as transforms data for display.
/// </summary>

public partial class MealViewModel : ObservableObject
{
    // reference to the model
    private Meal _meal = new();
    public Meal GetMeal() => _meal;
    // reference to the repository (or API if you have an actual backend)
    private readonly MealRepository _mealRepository;
    // page specific shits
    private bool _hasChanges = false;
    public bool HasChanges
    {
        get => _hasChanges;
        set => SetProperty(ref _hasChanges, value);
    }

    public int? MealID { get => _meal.MealID; } 
    public int? OperatorID { get => _meal.OperatorID; }    

    private string _mealName = string.Empty;
    public string MealName
    {
        get => _mealName;
        set
        {
            if (_mealName != value)
            {
                _mealName = value;
                OnPropertyChanged();
                _meal.MealName = value;
                _hasChanges = true;
            }
        }
    }

    private string _description = string.Empty;
    public string Description 
    { 
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged();
                _meal.Description = value;
                _hasChanges = true;
            }
        } 
    }

    public decimal _mealPrice = 0m;
    public decimal MealPrice 
    { 
        get => _mealPrice;
        set
        {
            if (_mealPrice != value)
            { 
                _mealPrice = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedPrice));
                _meal.MealPrice = value;
                _hasChanges = true;
            }
        } 
    }

    private int _stockQuantity = 0;
    public int StockQuantity 
    { 
        get => _stockQuantity;
        set 
        { 
            if (_stockQuantity != value)
            {
                _stockQuantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StockStatus));
                OnPropertyChanged(nameof(IsAvailable));
                OnPropertyChanged(nameof(AvailabilityColor));
                OnPropertyChanged(nameof(StockQuantityText));
                OnPropertyChanged(nameof(LowStockWarningVisibility));
                _meal.StockQuantity = value;
                _hasChanges = true;
            }
        } 
    }

    private int _minOrderQuantity = 1;
    public int MinOrderQuantity 
    { 
        get => _minOrderQuantity;
        set
        {
            if (_minOrderQuantity != value)
            {
                _minOrderQuantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAvailable));
                OnPropertyChanged(nameof(AvailabilityColor));
                OnPropertyChanged(nameof(MinMaxOrderText)); 
                _meal.MinOrderQuantity = value;
                _hasChanges = true;
            }
        } 
    }

    private List<MealTagType> _mealTags = new List<MealTagType>();
    public List<MealTagType> MealTags
    {
        get => _mealTags;
        set
        {
            if (_mealTags != value)
            {
                _mealTags = value;
                OnPropertyChanged();
                _meal.MealTags = value;
                _hasChanges = true;
            }
        }
    }

    private DeliveryType _deliveryType;
    public DeliveryType DeliveryType 
    {
        get => _deliveryType;
        set
        {
            if (_deliveryType != value)
            {
                _deliveryType = value;
                OnPropertyChanged();
                _meal.DeliveryType = value;
                _hasChanges = true;
            }
        } 
    }

    // TECH DEBT: Implement CRUD for TS on Repository
    private string _imageSourceString = string.Empty;
    public string ImageSourceString 
    { 
        get => _imageSourceString;
        set 
        {
            if (_imageSourceString != value)
            {
                _imageSourceString = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MealImage));
                _meal.ImageSourceString = value;
                _hasChanges = true;
            }
        }
    }

    private DateTime _creationDate;
    public DateTime CreationDate
    {
        get => _creationDate;
        set
        {
            if (_creationDate != value)
            {
                _creationDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RelativeCreationTime)); 
            }
        }
    }


    // Helper viewmodels
    public BitmapImage? MealImage
    {
        get
        {
            if (!string.IsNullOrEmpty(_imageSourceString))
            {
                return new BitmapImage(new Uri(_imageSourceString));
            }
            return null;
        }
    }
    
    public string FormattedPrice => MealPrice.ToString("C");
    public bool IsAvailable => StockQuantity >= MinOrderQuantity;
    public string AvailabilityColor => IsAvailable ? "Green" : "Red";
    public string StockStatus
    {
        get
        {
            if (StockQuantity == 0) return "Out of Stock";
            if (StockQuantity <= 5) return "Low Stock";
            return "In Stock";
        }
    }

    public string RelativeCreationTime
    {
        get
        {
            var timeSpan = DateTime.UtcNow - CreationDate;
            if (timeSpan.TotalDays < 1) return "Today";
            if (timeSpan.TotalDays < 7) return $"{timeSpan.Days} days ago";
            if (timeSpan.TotalDays < 30) return $"{timeSpan.Days / 7} weeks ago";
            return $"{timeSpan.Days / 30} months ago";
        }
    }

    public string StockQuantityText => $"{StockQuantity} units";
    public string MinMaxOrderText => $"Min: {MinOrderQuantity} units";
    public Visibility LowStockWarningVisibility => StockQuantity <= 5 ? Visibility.Visible : Visibility.Collapsed;

    // RELAY COMMANDS
    // This is item-based commands that will directly communicate with the backend.
    [RelayCommand]
    public async Task CreateCommand()
    {
        if (_hasChanges && !MealID.HasValue)
        {
            await _mealRepository.CreateMealAsync(_meal);
            _hasChanges = false;
        } // add error handling
    }

    // PARENT DEPENDENCY INJECTION
    // Implemented if you have a parent VM element (like an Observable List).
    // It allows for the individual item element to modify itself and relay that event to its parent.
    // Operationally speaking, this means that if you have a list of Meals represented by MealViewModel,
    // then if you delete from that list (a.k.a. it's parent) then that also means you've deleted the 
    // actual item also FOR REALS.

    private readonly AdminMealListViewModel _parent;
    // add a case later where you grab the OperatorID so that it can be logged
    // remove this in the parent as well for syncing
    public RelayCommand DeleteCommand => new RelayCommand(() => _parent.DeleteMeal(this));
    public RelayCommand RequestUpdateCommand => new RelayCommand(() =>
                                        _parent.UpdateSelectedMealCommand.Execute(null));

    // INITIAL DATA LOADING (via constructor injection)
    // This is what feeds the actual data to your VM
    public MealViewModel(Meal meal, AdminMealListViewModel parent) 
    {
        _meal = meal ?? new Meal();
        _parent = parent;
        _mealRepository = _parent.MealRepository;
        _hasChanges = false;

        _mealName = _meal.MealName ?? string.Empty;
        _description = _meal.Description ?? string.Empty;
        _mealPrice = _meal.MealPrice;
        _stockQuantity = _meal.StockQuantity;
        _mealTags = (List<MealTagType>)_meal.MealTags;
        _deliveryType = _meal.DeliveryType;
        _imageSourceString = _meal.ImageSourceString;
        _creationDate = _meal.CreationDate;
    }

    // Implement your own interface here twin, or just READ/bbbbbbbbbbbbbbbbbbbbbbbbbget{} the data
}
