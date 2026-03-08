using PESYONG.Domain.Entities.Financial.Promos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

public sealed class PromoViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private int? _promoId;
    private string _code = string.Empty;
    private string _description = string.Empty;
    private decimal _discountPercentageValue;
    private string _minimumOrderAmountText = string.Empty;
    private string _usageLimitText = string.Empty;
    private int _usedCount;
    private DateTime _validFrom = DateTime.Today;
    private DateTime _validUntil = DateTime.Today.AddDays(30);
    private string _statusMessage = string.Empty;

    private readonly Dictionary<string, List<string>> _errors = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public int? PromoID
    {
        get => _promoId;
        set => SetProperty(ref _promoId, value);
    }

    public string Code
    {
        get => _code;
        set
        {
            var normalized = (value ?? string.Empty).Trim().ToUpperInvariant();
            if (SetProperty(ref _code, normalized))
            {
                ValidateProperty(nameof(Code));
            }
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            if (SetProperty(ref _description, value ?? string.Empty))
            {
                ValidateProperty(nameof(Description));
            }
        }
    }

    public decimal DiscountPercentageValue
    {
        get => _discountPercentageValue;
        set
        {
            if (SetProperty(ref _discountPercentageValue, value))
            {
                ValidateProperty(nameof(DiscountPercentageValue));
                RefreshComputedState();
            }
        }
    }

    public string MinimumOrderAmountText
    {
        get => _minimumOrderAmountText;
        set
        {
            if (SetProperty(ref _minimumOrderAmountText, value ?? string.Empty))
            {
                ValidateProperty(nameof(MinimumOrderAmountText));
            }
        }
    }

    public string UsageLimitText
    {
        get => _usageLimitText;
        set
        {
            if (SetProperty(ref _usageLimitText, value ?? string.Empty))
            {
                ValidateProperty(nameof(UsageLimitText));
                RefreshComputedState();
            }
        }
    }

    public int UsedCount
    {
        get => _usedCount;
        set
        {
            if (SetProperty(ref _usedCount, value))
            {
                ValidateProperty(nameof(UsedCount));
                RefreshComputedState();
            }
        }
    }

    public DateTime ValidFrom
    {
        get => _validFrom;
        set
        {
            if (SetProperty(ref _validFrom, value))
            {
                ValidateProperty(nameof(ValidFrom));
                ValidateProperty(nameof(ValidUntil));
                RefreshComputedState();
            }
        }
    }

    public DateTime ValidUntil
    {
        get => _validUntil;
        set
        {
            if (SetProperty(ref _validUntil, value))
            {
                ValidateProperty(nameof(ValidUntil));
                ValidateProperty(nameof(ValidFrom));
                RefreshComputedState();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public decimal? MinimumOrderAmount
    {
        get
        {
            if (string.IsNullOrWhiteSpace(MinimumOrderAmountText))
                return null;

            return decimal.TryParse(
                MinimumOrderAmountText,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var parsedInvariant)
                ? parsedInvariant
                : decimal.TryParse(
                    MinimumOrderAmountText,
                    NumberStyles.Number,
                    CultureInfo.CurrentCulture,
                    out var parsedCurrent)
                    ? parsedCurrent
                    : null;
        }
    }

    public int? UsageLimit
    {
        get
        {
            if (string.IsNullOrWhiteSpace(UsageLimitText))
                return null;

            return int.TryParse(UsageLimitText.Trim(), out var parsed) ? parsed : null;
        }
    }

    public int AvailableCount
    {
        get
        {
            if (!UsageLimit.HasValue)
                return int.MaxValue;

            return UsageLimit.Value - UsedCount;
        }
    }

    public bool IsExpired => DateTime.Today > ValidUntil.Date;

    public bool IsDepleted => UsageLimit.HasValue && UsedCount >= UsageLimit.Value;

    public bool IsActive => !IsExpired && !IsDepleted;

    public string AvailabilityText => UsageLimit.HasValue ? AvailableCount.ToString() : "Unlimited";

    public string PromoStateText
    {
        get
        {
            if (IsExpired)
                return "Expired";
            if (IsDepleted)
                return "Depleted";
            return "Active";
        }
    }

    public string FormattedDiscountPercentage => $"{DiscountPercentageValue:N2}%";

    public string ValidationSummary
    {
        get
        {
            var messages = _errors.Values.SelectMany(x => x).Distinct().ToList();
            return messages.Count == 0 ? string.Empty : string.Join(Environment.NewLine, messages);
        }
    }

    public bool HasErrors => _errors.Count > 0;

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

    public void ClearPromoViewModel()
    {
        PromoID = null;
        Code = string.Empty;
        Description = string.Empty;
        DiscountPercentageValue = 0;
        MinimumOrderAmountText = string.Empty;
        UsageLimitText = string.Empty;
        UsedCount = 0;
        ValidFrom = DateTime.Today;
        ValidUntil = DateTime.Today.AddDays(30);
        StatusMessage = string.Empty;
        ClearAllErrors();
        RefreshComputedState();
    }

    public void LoadFromEntity(Promo entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        PromoID = entity.PromoID;
        Code = entity.Code ?? string.Empty;
        Description = entity.Description ?? string.Empty;
        DiscountPercentageValue = entity.DiscountPercentageValue;
        MinimumOrderAmountText = entity.MinimumOrderAmount?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        UsageLimitText = entity.UsageLimit?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        UsedCount = entity.UsedCount;
        ValidFrom = entity.ValidFrom;
        ValidUntil = entity.ValidUntil;
        StatusMessage = string.Empty;

        ValidateAll();
        RefreshComputedState();
    }

    public Promo ToEntity()
    {
        ValidateAll();

        if (HasErrors)
            throw new InvalidOperationException("Promo data is invalid. Review the validation messages before saving.");

        return new Promo
        {
            PromoID = PromoID ?? 0,
            Code = Code.Trim().ToUpperInvariant(),
            Description = Description.Trim(),
            DiscountPercentageValue = DiscountPercentageValue,
            MinimumOrderAmount = MinimumOrderAmount,
            UsageLimit = UsageLimit,
            UsedCount = UsedCount,
            ValidFrom = ValidFrom,
            ValidUntil = ValidUntil
        };
    }

    public static PromoViewModel CreateFromEntity(Promo entity)
    {
        var vm = new PromoViewModel();
        vm.LoadFromEntity(entity);
        return vm;
    }

    public bool ValidateAll()
    {
        ValidateProperty(nameof(Code));
        ValidateProperty(nameof(Description));
        ValidateProperty(nameof(DiscountPercentageValue));
        ValidateProperty(nameof(MinimumOrderAmountText));
        ValidateProperty(nameof(UsageLimitText));
        ValidateProperty(nameof(UsedCount));
        ValidateProperty(nameof(ValidFrom));
        ValidateProperty(nameof(ValidUntil));

        OnPropertyChanged(nameof(ValidationSummary));
        return !HasErrors;
    }

    public void RefreshComputedState()
    {
        OnPropertyChanged(nameof(MinimumOrderAmount));
        OnPropertyChanged(nameof(UsageLimit));
        OnPropertyChanged(nameof(AvailableCount));
        OnPropertyChanged(nameof(IsExpired));
        OnPropertyChanged(nameof(IsDepleted));
        OnPropertyChanged(nameof(IsActive));
        OnPropertyChanged(nameof(AvailabilityText));
        OnPropertyChanged(nameof(PromoStateText));
        OnPropertyChanged(nameof(FormattedDiscountPercentage));
        OnPropertyChanged(nameof(ValidationSummary));
    }

    private void ValidateProperty(string propertyName)
    {
        ClearErrors(propertyName);

        switch (propertyName)
        {
            case nameof(Code):
                if (string.IsNullOrWhiteSpace(Code))
                {
                    AddError(propertyName, "Promo code is required.");
                }
                else if (Code.Length > 20)
                {
                    AddError(propertyName, "Promo code must not exceed 20 characters.");
                }
                else if (!Code.All(ch => char.IsUpper(ch) || char.IsDigit(ch)))
                {
                    AddError(propertyName, "Promo code can only contain uppercase letters and numbers.");
                }
                break;

            case nameof(Description):
                if (string.IsNullOrWhiteSpace(Description))
                {
                    AddError(propertyName, "Description is required.");
                }
                else if (Description.Trim().Length > 100)
                {
                    AddError(propertyName, "Description must not exceed 100 characters.");
                }
                break;

            case nameof(DiscountPercentageValue):
                if (DiscountPercentageValue < 0.01m || DiscountPercentageValue > 100.00m)
                {
                    AddError(propertyName, "Discount percentage must be between 0.01 and 100.00.");
                }
                break;

            case nameof(MinimumOrderAmountText):
                if (!string.IsNullOrWhiteSpace(MinimumOrderAmountText))
                {
                    if (MinimumOrderAmount is null)
                    {
                        AddError(propertyName, "Minimum order amount must be a valid number or left blank.");
                    }
                    else if (MinimumOrderAmount < 0.01m || MinimumOrderAmount > 10000.00m)
                    {
                        AddError(propertyName, "Minimum order amount must be between 0.01 and 10000.00.");
                    }
                }
                break;

            case nameof(UsageLimitText):
                if (!string.IsNullOrWhiteSpace(UsageLimitText))
                {
                    if (UsageLimit is null)
                    {
                        AddError(propertyName, "Usage limit must be a valid whole number or left blank.");
                    }
                    else if (UsageLimit < 1 || UsageLimit > 100000)
                    {
                        AddError(propertyName, "Usage limit must be between 1 and 100000.");
                    }
                }
                break;

            case nameof(UsedCount):
                if (UsedCount < 0 || UsedCount > 100000)
                {
                    AddError(propertyName, "Used count must be between 0 and 100000.");
                }
                break;

            case nameof(ValidFrom):
            case nameof(ValidUntil):
                if (ValidUntil.Date < ValidFrom.Date)
                {
                    AddError(nameof(ValidUntil), "Valid until date must be on or after valid from date.");
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
}