
using Microsoft.UI.Xaml.Media.Imaging;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Meals.MealProduct;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

public sealed class MealProductViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private int? _mealProductId;
    private string? _ownerId;
    private int? _promoId;
    private bool _isCateringPackage;
    private string _productName = string.Empty;
    private string? _productDescription;
    private byte[]? _imageBytes;
    private BitmapImage? _productImage;
    private int _paxCount;
    private bool _isAvailable = true;
    private bool _isCustomizable;
    private int _preferredViandCount;
    private DateTime _creationDate = DateTime.UtcNow;
    private DateTime _lastModifiedDate = DateTime.UtcNow;
    private string _statusMessage = string.Empty;
    private Meal? _selectedMealForAdd;

    private readonly Dictionary<string, List<string>> _errors = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public MealProductViewModel()
    {
        MealProductItems = new ObservableCollection<MealProductItemViewModel>();
        AvailableMeals = new ObservableCollection<Meal>();

        MealProductItems.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(ItemCount));
            OnPropertyChanged(nameof(ProductBasePrice));
            OnPropertyChanged(nameof(FinalPrice));
            OnPropertyChanged(nameof(DiscountAmount));
            OnPropertyChanged(nameof(FormattedProductBasePrice));
            OnPropertyChanged(nameof(FormattedFinalPrice));
            OnPropertyChanged(nameof(FormattedDiscountAmount));
            OnPropertyChanged(nameof(CustomizableDisplay));
            OnPropertyChanged(nameof(ValidationSummary));
        };
    }

    public int? MealProductID
    {
        get => _mealProductId;
        set => SetProperty(ref _mealProductId, value);
    }

    public string? OwnerID
    {
        get => _ownerId;
        set
        {
            if (SetProperty(ref _ownerId, value))
            {
                ValidateProperty(nameof(OwnerID));
            }
        }
    }

    public int? PromoID
    {
        get => _promoId;
        set
        {
            if (SetProperty(ref _promoId, value))
            {
                ValidateProperty(nameof(PromoID));
            }
        }
    }

    public bool IsCateringPackage
    {
        get => _isCateringPackage;
        set => SetProperty(ref _isCateringPackage, value);
    }

    public string ProductName
    {
        get => _productName;
        set
        {
            if (SetProperty(ref _productName, value))
            {
                ValidateProperty(nameof(ProductName));
            }
        }
    }

    public string? ProductDescription
    {
        get => _productDescription;
        set
        {
            if (SetProperty(ref _productDescription, value))
            {
                ValidateProperty(nameof(ProductDescription));
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
                _ = UpdateProductImageAsync(value);
            }
        }
    }

    public DateTime CreationDate
    {
        get => _creationDate;
        set => SetProperty(ref _creationDate, value);
    }

    public DateTime LastModifiedDate
    {
        get => _lastModifiedDate;
        set => SetProperty(ref _lastModifiedDate, value);
    }

    public int PaxCount
    {
        get => _paxCount;
        set
        {
            if (SetProperty(ref _paxCount, value))
            {
                ValidateProperty(nameof(PaxCount));
                OnPropertyChanged(nameof(PaxDisplay));
            }
        }
    }

    public string PaxDisplay => PaxCount > 0 ? $"Good for {PaxCount} pax" : string.Empty;

    public bool IsAvailable
    {
        get => _isAvailable;
        set => SetProperty(ref _isAvailable, value);
    }

    public BitmapImage? ProductImage
    {
        get => _productImage;
        private set => SetProperty(ref _productImage, value);
    }

    public bool IsCustomizable
    {
        get => _isCustomizable;
        set
        {
            if (SetProperty(ref _isCustomizable, value))
            {
                ValidateProperty(nameof(IsCustomizable));
                ValidateProperty(nameof(PreferredViandCount));
                OnPropertyChanged(nameof(CustomizableDisplay));
                OnPropertyChanged(nameof(ProductBasePrice));
                OnPropertyChanged(nameof(FinalPrice));
                OnPropertyChanged(nameof(FormattedProductBasePrice));
                OnPropertyChanged(nameof(FormattedFinalPrice));
                OnPropertyChanged(nameof(ValidationSummary));
            }
        }
    }

    public int PreferredViandCount
    {
        get => _preferredViandCount;
        set
        {
            if (SetProperty(ref _preferredViandCount, value))
            {
                ValidateProperty(nameof(PreferredViandCount));
                OnPropertyChanged(nameof(CustomizableDisplay));
                OnPropertyChanged(nameof(ValidationSummary));
            }
        }
    }

    public string CustomizableDisplay =>
        IsCustomizable ? $"Customizable ({PreferredViandCount} viands)" : "Fixed";
    public ObservableCollection<MealProductItemViewModel> MealProductItems { get; }

    public ObservableCollection<Meal> AvailableMeals { get; }

    public Meal? SelectedMealForAdd
    {
        get => _selectedMealForAdd;
        set => SetProperty(ref _selectedMealForAdd, value);
    }

    public int ItemCount => MealProductItems.Count;

    public decimal ProductBasePrice => IsCustomizable
    ? 0
    : MealProductItems.Sum(x => x.ItemPrice);

    public decimal FinalPrice => ProductBasePrice;

    public decimal DiscountAmount => ProductBasePrice - FinalPrice;

    public string FormattedProductBasePrice => $"PHP {ProductBasePrice:N2}";

    public string FormattedFinalPrice => $"PHP {FinalPrice:N2}";

    public string FormattedDiscountAmount => $"PHP {DiscountAmount:N2}";

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string ValidationSummary
    {
        get
        {
            var messages = _errors.Values.SelectMany(x => x).ToList();

            foreach (var item in MealProductItems)
            {
                if (!item.IsValid())
                {
                    if (!string.IsNullOrWhiteSpace(item[nameof(item.MealID)]))
                        messages.Add(item[nameof(item.MealID)]);

                    if (!string.IsNullOrWhiteSpace(item[nameof(item.Quantity)]))
                        messages.Add(item[nameof(item.Quantity)]);

                    if (!string.IsNullOrWhiteSpace(item[nameof(item.RequestDescription)]))
                        messages.Add(item[nameof(item.RequestDescription)]);
                }
            }

            return string.Join(Environment.NewLine, messages.Distinct());
        }
    }

    public bool HasErrors => _errors.Count > 0 || MealProductItems.Any(x => !x.IsValid());

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return _errors.Values.SelectMany(x => x).ToList();
        }

        return _errors.TryGetValue(propertyName, out var list)
            ? list
            : Enumerable.Empty<string>();
    }

    public void ClearMealProductViewModel()
    {
        MealProductID = null;
        OwnerID = null;
        PromoID = null;
        IsCateringPackage = false;
        IsAvailable = true;
        PaxCount = 0;
        IsCustomizable = false;
        PreferredViandCount = 0;
        ProductName = string.Empty;
        ProductDescription = string.Empty;
        ImageBytes = null;
        CreationDate = DateTime.UtcNow;
        LastModifiedDate = DateTime.UtcNow;
        SelectedMealForAdd = null;
        MealProductItems.Clear();
        StatusMessage = string.Empty;
        ClearAllErrors();
    }

    public void SetAvailableMeals(IEnumerable<Meal> meals)
    {
        AvailableMeals.Clear();

        foreach (var meal in meals.OrderBy(x => x.MealName))
        {
            AvailableMeals.Add(meal);
        }
    }

    public void AddMealItem(Meal meal)
    {
        if (meal == null)
            return;

        var existing = MealProductItems.FirstOrDefault(x => x.MealID == meal.MealID);
        if (existing != null)
        {
            existing.Quantity += 1;
            OnPropertyChanged(nameof(ProductBasePrice));
            OnPropertyChanged(nameof(FinalPrice));
            OnPropertyChanged(nameof(DiscountAmount));
            OnPropertyChanged(nameof(FormattedProductBasePrice));
            OnPropertyChanged(nameof(FormattedFinalPrice));
            OnPropertyChanged(nameof(FormattedDiscountAmount));
            OnPropertyChanged(nameof(ValidationSummary));
            return;
        }

        var vm = new MealProductItemViewModel
        {
            MealID = meal.MealID ?? 0,
            MealName = meal.MealName ?? $"Meal #{meal.MealID}",
            Quantity = 1,
            RequestDescription = null,
            UnitPrice = meal.MealPrice,
            MealReference = null
        };

        vm.PropertyChanged += MealItem_PropertyChanged;
        MealProductItems.Add(vm);

        OnPropertyChanged(nameof(ProductBasePrice));
        OnPropertyChanged(nameof(FinalPrice));
        OnPropertyChanged(nameof(DiscountAmount));
        OnPropertyChanged(nameof(FormattedProductBasePrice));
        OnPropertyChanged(nameof(FormattedFinalPrice));
        OnPropertyChanged(nameof(FormattedDiscountAmount));
        OnPropertyChanged(nameof(ValidationSummary));
    }

    public void RemoveMealItem(MealProductItemViewModel? item)
    {
        if (item == null)
            return;

        item.PropertyChanged -= MealItem_PropertyChanged;
        MealProductItems.Remove(item);

        OnPropertyChanged(nameof(ProductBasePrice));
        OnPropertyChanged(nameof(FinalPrice));
        OnPropertyChanged(nameof(DiscountAmount));
        OnPropertyChanged(nameof(FormattedProductBasePrice));
        OnPropertyChanged(nameof(FormattedFinalPrice));
        OnPropertyChanged(nameof(FormattedDiscountAmount));
        OnPropertyChanged(nameof(ValidationSummary));
    }

    public bool ValidateAll()
    {
        ValidateProperty(nameof(OwnerID));
        ValidateProperty(nameof(PromoID));
        ValidateProperty(nameof(ProductName));
        ValidateProperty(nameof(ProductDescription));
        ValidateProperty(nameof(PaxCount));

        if (IsCustomizable)
        {
            ClearErrors(nameof(MealProductItems));

            if (PreferredViandCount <= 0)
            {
                AddError(nameof(PreferredViandCount), "Preferred viand count must be greater than 0 for customizable packages.");
            }
            else
            {
                ClearErrors(nameof(PreferredViandCount));
            }
        }
        else
        {
            ClearErrors(nameof(PreferredViandCount));

            if (MealProductItems.Count == 0)
            {
                AddError(nameof(MealProductItems), "At least one meal item is required for a fixed package.");
            }
            else
            {
                ClearErrors(nameof(MealProductItems));
            }
        }

        OnPropertyChanged(nameof(ValidationSummary));
        return !HasErrors;
    }

    public void LoadFromEntity(MealProduct entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        MealProductID = entity.MealProductID;
        OwnerID = entity.OwnerID?.ToString();   
        PromoID = entity.PromoID;
        IsCateringPackage = entity.IsCateringPackage;
        IsCustomizable = entity.IsCustomizable;
        PreferredViandCount = entity.PreferredViandCount;
        IsAvailable = entity.IsAvailable;
        PaxCount = entity.PaxCount;
        ProductName = entity.ProductName ?? string.Empty;
        ProductDescription = entity.ProductDescription;
        ImageBytes = entity.ImageBytes;
        CreationDate = DateTime.UtcNow;
        LastModifiedDate = DateTime.UtcNow;

        foreach (var existing in MealProductItems.ToList())
        {
            existing.PropertyChanged -= MealItem_PropertyChanged;
        }

        MealProductItems.Clear();

        if (entity.MealProductItems != null)
        {
            foreach (var item in entity.MealProductItems)
            {
                var itemVm = MealProductItemViewModel.CreateFromEntity(item);
                itemVm.PropertyChanged += MealItem_PropertyChanged;
                MealProductItems.Add(itemVm);
            }
        }

        SelectedMealForAdd = null;
        StatusMessage = string.Empty;
        ValidateAll();

        OnPropertyChanged(nameof(ProductBasePrice));
        OnPropertyChanged(nameof(FinalPrice));
        OnPropertyChanged(nameof(DiscountAmount));
        OnPropertyChanged(nameof(FormattedProductBasePrice));
        OnPropertyChanged(nameof(FormattedFinalPrice));
        OnPropertyChanged(nameof(FormattedDiscountAmount));
        OnPropertyChanged(nameof(PaxDisplay));
        OnPropertyChanged(nameof(CustomizableDisplay));
    }

    public MealProduct ToEntity()
    {
        ValidateAll();

        if (HasErrors)
        {
            throw new InvalidOperationException("Meal product data is invalid. Review the form before saving.");
        }

        return new MealProduct
        {
            MealProductID = MealProductID ?? 0,
            OwnerID = ParseNullableInt(OwnerID),
            PromoID = PromoID,
            IsCateringPackage = IsCateringPackage,
            IsCustomizable = IsCustomizable,
            PreferredViandCount = PreferredViandCount,
            IsAvailable = IsAvailable,
            PaxCount = PaxCount,
            ProductName = ProductName.Trim(),
            ProductDescription = string.IsNullOrWhiteSpace(ProductDescription)
                ? null
                : ProductDescription.Trim(),
            ImageBytes = ImageBytes,
            MealProductItems = MealProductItems.Select(x => x.ToEntity()).ToList()
        };
    }

    public static MealProductViewModel CreateFromEntity(MealProduct entity)
    {
        var vm = new MealProductViewModel();
        vm.LoadFromEntity(entity);
        return vm;
    }

    private void MealItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(ProductBasePrice));
        OnPropertyChanged(nameof(FinalPrice));
        OnPropertyChanged(nameof(DiscountAmount));
        OnPropertyChanged(nameof(FormattedProductBasePrice));
        OnPropertyChanged(nameof(FormattedFinalPrice));
        OnPropertyChanged(nameof(FormattedDiscountAmount));
        OnPropertyChanged(nameof(ValidationSummary));
    }

    private void ValidateProperty(string propertyName)
    {
        ClearErrors(propertyName);

        switch (propertyName)
        {
            case nameof(OwnerID):
                if (!string.IsNullOrWhiteSpace(OwnerID) && ParseNullableInt(OwnerID) is null)
                {
                    AddError(propertyName, "Owner ID must be a valid whole number or left blank.");
                }
                break;

            case nameof(PromoID):
                if (PromoID.HasValue && PromoID.Value <= 0)
                    AddError(propertyName, "Promo ID must be a valid positive number or blank.");
                break;

            case nameof(ProductName):
                if (string.IsNullOrWhiteSpace(ProductName))
                {
                    AddError(propertyName, "Product name is required.");
                }
                else if (ProductName.Trim().Length > 100)
                {
                    AddError(propertyName, "Product name must not exceed 100 characters.");
                }
                break;

            case nameof(ProductDescription):
                if (!string.IsNullOrWhiteSpace(ProductDescription) && ProductDescription.Trim().Length > 100)
                {
                    AddError(propertyName, "Product description must not exceed 100 characters.");
                }
                break;
            case nameof(PaxCount):
                if (PaxCount < 0)
                {
                    AddError(propertyName, "Number of pax cannot be negative.");
                }
                break;
            case nameof(IsCustomizable):
                // no direct validation needed here
                break;

            case nameof(PreferredViandCount):
                if (IsCustomizable && PreferredViandCount <= 0)
                {
                    AddError(propertyName, "Preferred viand count must be greater than 0.");
                }
                break;
        }

        OnPropertyChanged(nameof(ValidationSummary));
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
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async Task UpdateProductImageAsync(byte[]? bytes)
    {
        try
        {
            if (bytes == null || bytes.Length == 0)
            {
                ProductImage = null;
                return;
            }

            using var memoryStream = new System.IO.MemoryStream(bytes);
            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(memoryStream.AsRandomAccessStream());
            ProductImage = bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateProductImageAsync failed: {ex}");
            ProductImage = null;
        }
    }

    private static int? ParseNullableInt(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        return int.TryParse(input.Trim(), out var parsed)
            ? parsed
            : null;
    }
}