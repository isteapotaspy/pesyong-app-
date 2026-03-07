using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Enums;

namespace PESYONG.ApplicationLogic.ViewModels.ObjectModels;

public partial class MealViewModel : ObservableValidator
{
    private readonly MealRepository? _mealRepository;

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
    private int? lastModifiedByOperatorID;

    [ObservableProperty]
    private DateTime lastModifiedDate = DateTime.UtcNow;

    private BitmapImage _mealImage = new BitmapImage();
    public BitmapImage MealImage
    {
        get => _mealImage;
        set => SetProperty(ref _mealImage, value);
    }

    private byte[]? imageBytes;
    public byte[]? ImageBytes
    {
        get => imageBytes;
        set
        {
            if (SetProperty(ref imageBytes, value))
            {
                _ = LoadMealImageFromBytesAsync(value);
            }
        }
    }

    [ObservableProperty]
    private ObservableCollection<string> selectedTags = new();

    [ObservableProperty]
    private ObservableCollection<string> availableTags = new();

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

    public MealViewModel(MealRepository mealService)
    {
        _mealRepository = mealService;

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

        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName != nameof(HasValidationErrors) &&
                e.PropertyName != nameof(ValidationErrors) &&
                e.PropertyName != nameof(MealImage))
            {
                Validate();
                SaveCommand.NotifyCanExecuteChanged();
            }
        };
    }

    public static MealViewModel CreateFromEntity(Meal meal, MealRepository repository)
    {
        var vm = new MealViewModel(repository);
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
        ImageBytes = meal.ImageBytes;

        MealTags.Clear();
        foreach (var tag in meal.MealTags)
        {
            MealTags.Add(tag);
        }
    }

    public Meal ToEntity()
    {
        return new Meal
        {
            MealID = MealID,
            OperatorID = OperatorID,
            MealName = MealName,
            Description = string.IsNullOrWhiteSpace(Description) ? null : Description,
            MealPrice = MealPrice,
            StockQuantity = StockQuantity,
            MinOrderQuantity = MinOrderQuantity,
            DeliveryType = DeliveryType,
            CreationDate = CreationDate,
            LastModifiedByOperatorID = LastModifiedByOperatorID,
            LastModifiedDate = LastModifiedDate,
            ImageBytes = ImageBytes,
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
        MealName = string.Empty;
        Description = string.Empty;
        MealPrice = 0;
        StockQuantity = 0;
        MinOrderQuantity = 1;
        DeliveryType = DeliveryType.Delivery;
        CreationDate = DateTime.UtcNow;
        LastModifiedByOperatorID = null;
        LastModifiedDate = DateTime.UtcNow;
        ImageBytes = null;
        MealImage = new BitmapImage();
        MealTags = new ObservableCollection<string>();
    }

    private async Task UploadImageAsync()
    {
        // keep empty for now if page code-behind handles image picking
        await Task.CompletedTask;
    }

    private async Task SaveMealAsync()
    {
        if (!CanSaveMeal() || _mealRepository == null) return;

        try
        {
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
            ShowEventOnDebugConsole("Error", $"An error occurred while saving meal: {ex.Message}", "OK");
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
            ShowEventOnDebugConsole("Error", $"Failed to load meal: {ex.Message}", "OK");
        }
    }

    private async Task DeleteMealAsync()
    {
        if (!MealID.HasValue || _mealRepository == null) return;

        try
        {
            await _mealRepository.DeleteMealAsync(MealID.Value);
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", $"An error occurred while deleting meal: {ex.Message}", "OK");
        }
    }

    private async Task LoadMealImageFromBytesAsync(byte[]? bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            MealImage = new BitmapImage();
            return;
        }

        try
        {
            var bitmap = new BitmapImage();

            using var stream = new MemoryStream(bytes);
            using var randomAccessStream = stream.AsRandomAccessStream();
            await bitmap.SetSourceAsync(randomAccessStream);

            MealImage = bitmap;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load image from bytes: {ex.Message}");
            MealImage = new BitmapImage();
        }
    }

    private void Validate()
    {
        var entity = ToEntity();
        var errors = entity.GetValidationErrors().ToList();

        ValidationErrors.Clear();
        foreach (var error in errors)
        {
            ValidationErrors.Add(error ?? "Validation error");
        }

        HasValidationErrors = errors.Any();
    }

    private void ShowEventOnDebugConsole(string a, string b, string c)
    {
        Debug.Write($"[{a}] {c} : {b}");
    }

    public string RelativeCreationTime
    {
        get
        {
            var span = DateTime.UtcNow - CreationDate;

            if (span.TotalMinutes < 1) return "Just created";
            if (span.TotalHours < 1) return $"{(int)span.TotalMinutes} minute(s) ago";
            if (span.TotalDays < 1) return $"{(int)span.TotalHours} hour(s) ago";
            return $"{(int)span.TotalDays} day(s) ago";
        }
    }

    public string FormattedPrice => $"₱{MealPrice:N2}";

    public string StockStatus
    {
        get
        {
            if (StockQuantity <= 0) return "Out of Stock";
            if (StockQuantity <= 10) return "Low Stock";
            return "In Stock";
        }
    }

    public string StockQuantityText => $"{StockQuantity} units";


    partial void OnMealPriceChanged(decimal value)
    {
        OnPropertyChanged(nameof(FormattedPrice));
    }

    partial void OnStockQuantityChanged(int value)
    {
        OnPropertyChanged(nameof(StockStatus));
        OnPropertyChanged(nameof(StockQuantityText));
    }

    partial void OnCreationDateChanged(DateTime value)
    {
        OnPropertyChanged(nameof(RelativeCreationTime));
    }
}