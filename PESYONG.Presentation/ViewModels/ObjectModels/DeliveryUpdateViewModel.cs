using PESYONG.Domain.Entities.Logistics;
using PESYONG.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

public class DeliveryUpdateViewModel : INotifyPropertyChanged, IDataErrorInfo
{
    private int? _deliveryUpdateID;
    private int _deliveryID;
    private int? _updatedByUserID;
    private DeliveryStatus _status = DeliveryStatus.Pending;
    private DateTime _updateDate = DateTime.Now;
    private string _updateDescription = string.Empty;
    private string? _location;
    private string? _notes;
    private string _statusMessage = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int? DeliveryUpdateID
    {
        get => _deliveryUpdateID;
        set
        {
            if (SetProperty(ref _deliveryUpdateID, value))
            {
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    [Required(ErrorMessage = "Delivery ID is required.")]
    public int DeliveryID
    {
        get => _deliveryID;
        set => SetProperty(ref _deliveryID, value);
    }

    public int? UpdatedByUserID
    {
        get => _updatedByUserID;
        set => SetProperty(ref _updatedByUserID, value);
    }

    [Required(ErrorMessage = "Status is required.")]
    public DeliveryStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    [Required(ErrorMessage = "Update date is required.")]
    public DateTime UpdateDate
    {
        get => _updateDate;
        set => SetProperty(ref _updateDate, value);
    }

    [Required(ErrorMessage = "Update description is required.")]
    [StringLength(500, ErrorMessage = "Update description must not exceed 500 characters.")]
    public string UpdateDescription
    {
        get => _updateDescription;
        set => SetProperty(ref _updateDescription, value);
    }

    [StringLength(200, ErrorMessage = "Location must not exceed 200 characters.")]
    public string? Location
    {
        get => _location;
        set => SetProperty(ref _location, value);
    }

    [StringLength(1000, ErrorMessage = "Notes must not exceed 1000 characters.")]
    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (SetProperty(ref _statusMessage, value))
            {
                OnPropertyChanged(nameof(HasStatusMessage));
            }
        }
    }

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    public string DisplayName =>
        DeliveryUpdateID.HasValue
            ? $"{UpdateDate:g} - {Status}"
            : "New Delivery Update";

    // -----------------------------
    // UI Safe Wrapper Properties
    // -----------------------------
    public string DeliveryIDText
    {
        get => DeliveryID <= 0 ? string.Empty : DeliveryID.ToString();
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                DeliveryID = 0;
            }
            else if (int.TryParse(value.Trim(), out var parsedInt))
            {
                DeliveryID = parsedInt;
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(DeliveryID));
        }
    }

    public string UpdatedByUserIDText
    {
        get => UpdatedByUserID?.ToString() ?? string.Empty;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                UpdatedByUserID = null;
            }
            else if (int.TryParse(value.Trim(), out var parsedInt))
            {
                UpdatedByUserID = parsedInt;
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(UpdatedByUserID));
        }
    }

    public DateTimeOffset UpdateDateUi
    {
        get => new DateTimeOffset(UpdateDate);
        set
        {
            UpdateDate = value.DateTime;
            OnPropertyChanged();
        }
    }

    public string Error => string.Empty;

    public string this[string columnName]
    {
        get
        {
            try
            {
                if (columnName == nameof(DeliveryIDText))
                {
                    return DeliveryID <= 0 ? "Delivery ID is required." : string.Empty;
                }

                if (columnName == nameof(UpdatedByUserIDText))
                {
                    return string.Empty;
                }

                var context = new ValidationContext(this) { MemberName = columnName };
                var results = new List<ValidationResult>();

                var property = GetType().GetProperty(columnName);
                if (property == null)
                    return string.Empty;

                var value = property.GetValue(this);
                var valid = Validator.TryValidateProperty(value, context, results);

                return valid ? string.Empty : results.FirstOrDefault()?.ErrorMessage ?? string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Indexer validation error for {columnName}: {ex}");
                return string.Empty;
            }
        }
    }

    public static DeliveryUpdateViewModel CreateFromEntity(DeliveryUpdate entity)
    {
        var vm = new DeliveryUpdateViewModel();
        vm.LoadFromEntity(entity);
        return vm;
    }

    public void LoadFromEntity(DeliveryUpdate entity)
    {
        DeliveryUpdateID = entity.DeliveryUpdateID;
        DeliveryID = entity.DeliveryID;
        UpdatedByUserID = entity.UpdatedByUserID;
        Status = entity.Status;
        UpdateDate = entity.UpdateDate;
        UpdateDescription = entity.UpdateDescription;
        Location = entity.Location;
        Notes = entity.Notes;

        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(DeliveryIDText));
        OnPropertyChanged(nameof(UpdatedByUserIDText));
        OnPropertyChanged(nameof(UpdateDateUi));
    }

    public DeliveryUpdate ToEntity()
    {
        return new DeliveryUpdate
        {
            DeliveryUpdateID = DeliveryUpdateID ?? 0,
            DeliveryID = DeliveryID,
            UpdatedByUserID = UpdatedByUserID,
            Status = Status,
            UpdateDate = UpdateDate,
            UpdateDescription = UpdateDescription?.Trim() ?? string.Empty,
            Location = NormalizeNullable(Location),
            Notes = NormalizeNullable(Notes)
        };
    }

    public void ClearDeliveryUpdateViewModel()
    {
        DeliveryUpdateID = null;
        DeliveryID = 0;
        UpdatedByUserID = null;
        Status = DeliveryStatus.Pending;
        UpdateDate = DateTime.Now;
        UpdateDescription = string.Empty;
        Location = null;
        Notes = null;
        StatusMessage = string.Empty;

        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(DeliveryIDText));
        OnPropertyChanged(nameof(UpdatedByUserIDText));
        OnPropertyChanged(nameof(UpdateDateUi));
    }

    public bool ValidateAll()
    {
        try
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(this);
            var isValid = Validator.TryValidateObject(this, context, results, true);

            if (!isValid)
            {
                Debug.WriteLine("DeliveryUpdate validation failed:");
                foreach (var result in results)
                {
                    Debug.WriteLine(result.ErrorMessage);
                }
            }

            OnPropertyChanged(string.Empty);

            var businessRulesValid = ValidateBusinessRules();
            return isValid && businessRulesValid;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"DeliveryUpdate validation exception: {ex}");
            return false;
        }
    }

    private bool ValidateBusinessRules()
    {
        if (DeliveryID <= 0)
        {
            StatusMessage = "Delivery ID is required.";
            return false;
        }

        StatusMessage = string.Empty;
        return true;
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}