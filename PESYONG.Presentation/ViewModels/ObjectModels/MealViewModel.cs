using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PESYONG.ApplicationLogic.ViewModels.ObjectModels;

public sealed class MealViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private int? _mealId;
    private string _mealName = string.Empty;
    private string _mealPrice = string.Empty;
    private string _description = string.Empty;
    private byte[]? _imageBytes;
    private ImageSource? _mealImage;
    private double _stockQuantity;
    private double _minOrderQuantity = 1;
    private DeliveryType _deliveryType = DeliveryType.Delivery;
    private string? _operatorId;
    private string? _lastModifiedByOperatorId;
    private DateTime _creationDate = DateTime.UtcNow;
    private DateTime _lastModifiedDate = DateTime.UtcNow;
    private string _statusMessage = string.Empty;

    private readonly Dictionary<string, List<string>> _errors = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public MealViewModel()
    {
        MealTags = new ObservableCollection<string>();
        AvailableTags = new ObservableCollection<string>
        {
            "Makakalibanga",
            "Makapapurigit",
            "Best Seller",
            "Budget Meal",
            "Sweet",
            "Rice Cake",
            "Local Favorite",
            "Party Tray"
        };
    }

    public int? MealID
    {
        get => _mealId;
        set => SetProperty(ref _mealId, value);
    }

    public string MealName
    {
        get => _mealName;
        set
        {
            if (SetProperty(ref _mealName, value))
            {
                ValidateProperty(nameof(MealName));
            }
        }
    }

    public string MealPrice
    {
        get => _mealPrice;
        set
        {
            if (SetProperty(ref _mealPrice, value))
            {
                OnPropertyChanged(nameof(FormattedPrice));
                ValidateProperty(nameof(MealPrice));
            }
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            if (SetProperty(ref _description, value))
            {
                ValidateProperty(nameof(Description));
            }
        }
    }

    public byte[]? ImageBytes
    {
        get => _imageBytes;
        set
        {
            if (SetProperty(ref _imageBytes, value))
            {
                _ = UpdateMealImageAsync(value);
            }
        }
    }

    public ImageSource? MealImage
    {
        get => _mealImage;
        private set => SetProperty(ref _mealImage, value);
    }

    public double StockQuantity
    {
        get => _stockQuantity;
        set
        {
            if (SetProperty(ref _stockQuantity, value))
            {
                ValidateProperty(nameof(StockQuantity));
            }
        }
    }

    public double MinOrderQuantity
    {
        get => _minOrderQuantity;
        set
        {
            if (SetProperty(ref _minOrderQuantity, value))
            {
                ValidateProperty(nameof(MinOrderQuantity));
            }
        }
    }

    public DeliveryType DeliveryType
    {
        get => _deliveryType;
        set => SetProperty(ref _deliveryType, value);
    }

    public string? OperatorID
    {
        get => _operatorId;
        set
        {
            if (SetProperty(ref _operatorId, value))
            {
                ValidateProperty(nameof(OperatorID));
            }
        }
    }

    public string? LastModifiedByOperatorID
    {
        get => _lastModifiedByOperatorId;
        set => SetProperty(ref _lastModifiedByOperatorId, value);
    }

    public DateTime CreationDate
    {
        get => _creationDate;
        set
        {
            if (SetProperty(ref _creationDate, value))
            {
                OnPropertyChanged(nameof(RelativeCreationTime));
            }
        }
    }

    public DateTime LastModifiedDate
    {
        get => _lastModifiedDate;
        set => SetProperty(ref _lastModifiedDate, value);
    }

    public ObservableCollection<string> MealTags { get; }

    public ObservableCollection<string> AvailableTags { get; }

    public string FormattedPrice
    {
        get
        {
            return TryParsePrice(MealPrice, out var parsed)
                ? $"PHP {parsed:N2}"
                : "PHP 0.00";
        }
    }

    public string RelativeCreationTime
    {
        get
        {
            var now = DateTime.UtcNow;
            var span = now - CreationDate;

            if (span.TotalMinutes < 1)
                return "Just created";
            if (span.TotalHours < 1)
                return $"{Math.Max(1, (int)span.TotalMinutes)} minute(s) ago";
            if (span.TotalDays < 1)
                return $"{Math.Max(1, (int)span.TotalHours)} hour(s) ago";
            if (span.TotalDays < 30)
                return $"{Math.Max(1, (int)span.TotalDays)} day(s) ago";

            return CreationDate.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture);
        }
    }

    public string ValidationSummary
    {
        get
        {
            var messages = _errors.Values.SelectMany(x => x).Distinct().ToList();
            return messages.Count == 0
                ? string.Empty
                : string.Join(Environment.NewLine, messages);
        }
    }

    public bool HasValidationErrors => _errors.Count > 0;

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool HasErrors => _errors.Count > 0;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return _errors.Values.SelectMany(x => x).ToList();
        }

        return _errors.TryGetValue(propertyName, out var errors)
            ? errors
            : Enumerable.Empty<string>();
    }

    public void ClearMealViewModel()
    {
        MealID = null;
        MealName = string.Empty;
        MealPrice = string.Empty;
        Description = string.Empty;
        ImageBytes = null;
        StockQuantity = 0;
        MinOrderQuantity = 1;
        DeliveryType = DeliveryType.Delivery;
        OperatorID = null;
        LastModifiedByOperatorID = null;
        CreationDate = DateTime.UtcNow;
        LastModifiedDate = DateTime.UtcNow;
        MealTags.Clear();
        StatusMessage = string.Empty;

        ClearAllErrors();
        OnPropertyChanged(nameof(FormattedPrice));
        OnPropertyChanged(nameof(RelativeCreationTime));
    }

    public void LoadFromEntity(Meal entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        MealID = entity.MealID;
        MealName = entity.MealName ?? string.Empty;
        MealPrice = Convert.ToString(entity.MealPrice, CultureInfo.InvariantCulture) ?? string.Empty;
        Description = entity.Description ?? string.Empty;
        ImageBytes = entity.ImageBytes;
        StockQuantity = Convert.ToDouble(entity.StockQuantity, CultureInfo.InvariantCulture);
        MinOrderQuantity = Convert.ToDouble(entity.MinOrderQuantity, CultureInfo.InvariantCulture);
        DeliveryType = entity.DeliveryType;
        OperatorID = entity.OperatorID?.ToString();
        LastModifiedByOperatorID = entity.LastModifiedByOperatorID?.ToString();
        CreationDate = entity.CreationDate == default ? DateTime.UtcNow : entity.CreationDate;
        LastModifiedDate = entity.LastModifiedDate == default ? DateTime.UtcNow : entity.LastModifiedDate;

        MealTags.Clear();
        if (entity.MealTags != null)
        {
            foreach (var tag in entity.MealTags.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                MealTags.Add(tag.Trim());
            }
        }

        StatusMessage = string.Empty;
        ValidateAll();
    }

    public Meal ToEntity()
    {
        ValidateAll();

        if (HasErrors)
        {
            throw new InvalidOperationException("Meal data is invalid. Review the validation messages before saving.");
        }

        var parsedPrice = TryParsePrice(MealPrice, out var price)
            ? (int)Math.Round(price)
            : 0;

        return new Meal
        {
            MealID = MealID,
            MealName = MealName.Trim(),
            MealPrice = parsedPrice,
            Description = Description.Trim(),
            ImageBytes = ImageBytes,
            StockQuantity = (int)Math.Round(StockQuantity),
            MinOrderQuantity = (int)Math.Round(MinOrderQuantity),
            DeliveryType = DeliveryType,
            MealTags = MealTags
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            OperatorID = ParseNullableInt(OperatorID),
            LastModifiedByOperatorID = ParseNullableInt(LastModifiedByOperatorID),
            CreationDate = CreationDate,
            LastModifiedDate = LastModifiedDate
        };
    }

    public static MealViewModel CreateFromEntity(Meal entity)
    {
        var vm = new MealViewModel();
        vm.LoadFromEntity(entity);
        return vm;
    }

    public void RefreshComputedState()
    {
        OnPropertyChanged(nameof(FormattedPrice));
        OnPropertyChanged(nameof(RelativeCreationTime));
        OnPropertyChanged(nameof(ValidationSummary));
        OnPropertyChanged(nameof(HasValidationErrors));
    }

    public bool ValidateAll()
    {
        ValidateProperty(nameof(MealName));
        ValidateProperty(nameof(MealPrice));
        ValidateProperty(nameof(Description));
        ValidateProperty(nameof(StockQuantity));
        ValidateProperty(nameof(MinOrderQuantity));
        ValidateProperty(nameof(OperatorID));

        OnPropertyChanged(nameof(ValidationSummary));
        OnPropertyChanged(nameof(HasValidationErrors));

        return !HasErrors;
    }

    private void ValidateProperty(string propertyName)
    {
        ClearErrors(propertyName);

        switch (propertyName)
        {
            case nameof(MealName):
                if (string.IsNullOrWhiteSpace(MealName))
                {
                    AddError(propertyName, "Meal title is required.");
                }
                else if (MealName.Trim().Length < 2)
                {
                    AddError(propertyName, "Meal title must be at least 2 characters long.");
                }
                break;

            case nameof(MealPrice):
                if (string.IsNullOrWhiteSpace(MealPrice))
                {
                    AddError(propertyName, "Price is required.");
                }
                else if (!TryParsePrice(MealPrice, out var parsedPrice))
                {
                    AddError(propertyName, "Price must be a valid number.");
                }
                else if (parsedPrice < 0)
                {
                    AddError(propertyName, "Price cannot be negative.");
                }
                break;

            case nameof(Description):
                if (string.IsNullOrWhiteSpace(Description))
                {
                    AddError(propertyName, "Description is required.");
                }
                break;

            case nameof(StockQuantity):
                if (StockQuantity < 0)
                {
                    AddError(propertyName, "Stock quantity cannot be negative.");
                }
                break;

            case nameof(MinOrderQuantity):
                if (MinOrderQuantity < 1)
                {
                    AddError(propertyName, "Minimum order quantity must be at least 1.");
                }
                break;

            case nameof(OperatorID):
                if (!string.IsNullOrWhiteSpace(OperatorID) && ParseNullableInt(OperatorID) is null)
                {
                    AddError(propertyName, "Creator ID must be a valid whole number or left blank.");
                }
                break;
        }

        OnPropertyChanged(nameof(ValidationSummary));
        OnPropertyChanged(nameof(HasValidationErrors));
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if (!string.IsNullOrWhiteSpace(propertyName))
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private void AddError(string propertyName, string error)
    {
        if (!_errors.TryGetValue(propertyName, out var list))
        {
            list = new List<string>();
            _errors[propertyName] = list;
        }

        if (!list.Contains(error))
        {
            list.Add(error);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    private void ClearErrors(string propertyName)
    {
        if (_errors.Remove(propertyName))
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    private void ClearAllErrors()
    {
        var keys = _errors.Keys.ToList();
        _errors.Clear();

        foreach (var key in keys)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(key));
        }

        OnPropertyChanged(nameof(ValidationSummary));
        OnPropertyChanged(nameof(HasValidationErrors));
    }

    private static bool TryParsePrice(string? input, out decimal parsed)
    {
        return decimal.TryParse(
            input,
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands,
            CultureInfo.InvariantCulture,
            out parsed)
            || decimal.TryParse(
                input,
                NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands,
                CultureInfo.CurrentCulture,
                out parsed);
    }

    private static int? ParseNullableInt(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        return int.TryParse(input.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private async Task UpdateMealImageAsync(byte[]? bytes)
    {
        try
        {
            if (bytes == null || bytes.Length == 0)
            {
                MealImage = null;
                return;
            }

            using var stream = new InMemoryRandomAccessStream();
            using (var writer = new DataWriter(stream))
            {
                writer.WriteBytes(bytes);
                await writer.StoreAsync();
                await writer.FlushAsync();
            }

            stream.Seek(0);

            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);
            MealImage = bitmap;
        }
        catch
        {
            MealImage = null;
        }
    }
}