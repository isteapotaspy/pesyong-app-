using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media.Imaging;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Enums;
using PESYONG.Presentation;

namespace PESYONG.ApplicationLogic.ViewModels.ObjectModels;

public partial class MealViewModel : ObservableValidator
{
    private MealRepository _mealRepository;

    [ObservableProperty]
    private int? mealID;

    [ObservableProperty]
    private int? operatorID;

    [ObservableProperty]
    private ObservableCollection<string> mealTags = new();

    [Required]
    [ObservableProperty]
    private string mealName = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private decimal mealPrice;

    [ObservableProperty]
    private int stockQuantity;

    [ObservableProperty]
    private int minOrderQuantity = 1;

    [ObservableProperty]
    private DeliveryType deliveryType;

    [ObservableProperty]
    private DateTime creationDate = DateTime.UtcNow;

    [ObservableProperty]
    private int lastModifiedByOperatorID;

    [ObservableProperty]
    private DateTime lastModifiedDate = DateTime.UtcNow;

    [ObservableProperty]
    private string imageSourceString = string.Empty;

    private BitmapImage _mealImage = new BitmapImage();
    public BitmapImage MealImage
    {
        get => _mealImage;
        set => SetProperty(ref _mealImage, value);
    }

    [ObservableProperty]
    private ObservableCollection<string> selectedTags = new();

    [ObservableProperty]
    private ObservableCollection<string> availableTags = new();

    private byte[]? imageBytes;
    public byte[]? ImageBytes
    {
        get => imageBytes;
        set => SetProperty(ref imageBytes, value);
    }

    [ObservableProperty]
    private bool hasValidationErrors;

    [ObservableProperty]
    private ObservableCollection<string> validationErrors = new();

    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }
    public IRelayCommand AddTagCommand { get; }
    public IRelayCommand RemoveTagCommand { get; }
    public IAsyncRelayCommand UploadImageCommand { get; }

    public MealViewModel()
    {
        _mealRepository = App.Instance.Services.GetRequiredService<MealRepository>();

        SaveCommand = new AsyncRelayCommand(SaveMealAsync, CanSaveMeal);
        AddTagCommand = new RelayCommand<string>(AddTag, CanAddTag);
        RemoveTagCommand = new RelayCommand<string>(RemoveTag);
        UploadImageCommand = new AsyncRelayCommand(UploadImageAsync);
        LoadCommand = new AsyncRelayCommand(LoadMealAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteMealAsync);

        AvailableTags = new ObservableCollection<string>
        {
            "Vegetarian", "Vegan", "Gluten-Free", "Dairy-Free", "Spicy",
            "Low-Carb", "High-Protein", "Keto", "Paleo", "Organic", "Kosher", "Halal"
        };

        PropertyChanged += async (s, e) =>
        {
            if (e.PropertyName != nameof(HasValidationErrors) &&
                e.PropertyName != nameof(ValidationErrors))
            {
                Validate();
                SaveCommand.NotifyCanExecuteChanged();
            }

            if (e.PropertyName == nameof(ImageSourceString))
            {
                await LoadMealImageAsync();
            }
        };
    }

    public static MealViewModel CreateFromEntity(Meal meal)
    {
        var vm = new MealViewModel();
        vm.LoadFromEntity(meal);
        return vm;
    }

    public void LoadFromEntity(Meal meal)
    {
        MealID = meal.MealID;
        OperatorID = meal.OperatorID;
        MealName = meal.MealName;
        Description = meal.Description ?? string.Empty;
        MealPrice = meal.MealPrice;
        StockQuantity = meal.StockQuantity;
        MinOrderQuantity = meal.MinOrderQuantity;
        DeliveryType = meal.DeliveryType;
        CreationDate = meal.CreationDate;
        LastModifiedByOperatorID = meal.LastModifiedByOperatorID;
        LastModifiedDate = meal.LastModifiedDate;
        ImageSourceString = meal.ImageSourceString;

        mealTags.Clear();
        foreach (var tag in meal.MealTags)
        {
            mealTags.Add(tag);
        }
    }

    public Meal ToEntity()
    {
        return new Meal
        {
            MealID = MealID,
            OperatorID = OperatorID ?? 0,
            MealName = MealName,
            Description = string.IsNullOrWhiteSpace(Description) ? null : Description,
            MealPrice = MealPrice,
            StockQuantity = StockQuantity,
            MinOrderQuantity = MinOrderQuantity,
            DeliveryType = DeliveryType,
            CreationDate = CreationDate,
            LastModifiedByOperatorID = LastModifiedByOperatorID,
            LastModifiedDate = LastModifiedDate,
            ImageSourceString = ImageSourceString,
            MealTags = MealTags.ToList()
        };
    }

    private bool CanSaveMeal() => !HasValidationErrors;

    private void AddTag(string? tag)
    {
        if (!string.IsNullOrWhiteSpace(tag) && !MealTags.Contains(tag))
        {
            MealTags.Add(tag);
            OnPropertyChanged(nameof(MealTags));
        }
    }

    private bool CanAddTag(string? tag) => !string.IsNullOrWhiteSpace(tag) && !MealTags.Contains(tag);

    private void RemoveTag(string? tag)
    {
        if (!string.IsNullOrWhiteSpace(tag) && MealTags.Contains(tag))
        {
            MealTags.Remove(tag);
            OnPropertyChanged(nameof(MealTags));
        }
    }

    public void ClearMealViewModel()
    {
        MealID = null;
        OperatorID = null;
        MealName = String.Empty;
        Description = String.Empty;
        MealPrice = 0;
        StockQuantity = 0;
        MinOrderQuantity = 0;
        DeliveryType = DeliveryType.Delivery;
        CreationDate = DateTime.UtcNow;
        LastModifiedByOperatorID = 0;
        LastModifiedDate = DateTime.UtcNow;
        ImageSourceString = String.Empty;
        MealTags = new();
    }

    private async Task UploadImageAsync()
    {
        // Image upload logic
    }

    private async Task SaveMealAsync()
    {
        if (!CanSaveMeal() || _mealRepository == null) return;

        try
        {
            var currentOperatorId = GetCurrentOperatorId();
            if (MealID.HasValue)
            {
                await _mealRepository.UpdateMealAsync(ToEntity());
            }
            else
            {
                await _mealRepository.CreateMealAsync(ToEntity());
            }
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", "An error occurred while saving meal", "OK");
        }
    }

    private async Task LoadMealAsync()
    {
        if (!MealID.HasValue || _mealRepository == null) return;

        try
        {
            var meal = await _mealRepository.GetMealByIdAsync(MealID.Value);
            if (meal != null)
            {
                LoadFromEntity(meal);
            }
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", "Failed to load meal", "OK");
        }
    }

    private async Task DeleteMealAsync()
    {
        if (!MealID.HasValue || _mealRepository == null) return;

        bool confirmed = true;

        if (confirmed)
        {
            try
            {
                var currentOperatorId = GetCurrentOperatorId();
                await _mealRepository.DeleteMealAsync(MealID.Value);
            }
            catch (Exception ex)
            {
                ShowEventOnDebugConsole("Error", "An error occurred while deleting meal", "OK");
            }
        }
    }

    private async Task LoadMealImageAsync()
    {
        if (string.IsNullOrEmpty(ImageSourceString))
            return;

        try
        {
            MealImage = new BitmapImage(new Uri(ImageSourceString));
        }
        catch (Exception ex)
        {
            // Handle invalid URI or loading errors
            Debug.WriteLine($"Failed to load image: {ex.Message}");
        }
    }

    private int GetCurrentOperatorId()
    {
        return 1;
    }

    private void Validate()
    {
        var entity = ToEntity();
        var errors = entity.GetValidationErrors().ToList();

        ValidationErrors.Clear();
        foreach (var error in errors)
        {
            ValidationErrors.Add(error);
        }

        HasValidationErrors = errors.Any();
    }

    private void ShowEventOnDebugConsole(string a, string b, string c)
    {
        Debug.Write($"[{a}] {c} : {b}");
    }
}
