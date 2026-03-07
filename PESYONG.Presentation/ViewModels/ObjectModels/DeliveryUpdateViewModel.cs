using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PESYONG.Domain.Entities.Logistics;
using PESYONG.Domain.Enums;
using static Azure.Core.HttpHeader;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

public partial class DeliveryUpdateViewModel : ObservableValidator
{
    [ObservableProperty]
    private int _deliveryUpdateID;

    [ObservableProperty]
    [Required(ErrorMessage = "Delivery ID is required")]
    private int _deliveryID;

    [ObservableProperty]
    private DeliveryViewModel? _delivery;

    [ObservableProperty]
    private int? _updatedByUserID;

    [ObservableProperty]
    private AppUserViewModel? _updatedByUser;

    [ObservableProperty]
    [Required(ErrorMessage = "Status is required")]
    [NotifyPropertyChangedFor(nameof(StatusDisplay))]
    private DeliveryStatus _status;

    [ObservableProperty]
    [Required(ErrorMessage = "Update date is required")]
    private DateTime _updateDate = DateTime.Now;

    [ObservableProperty]
    [Required(ErrorMessage = "Update description is required")]
    [StringLength(500, ErrorMessage = "Update description cannot exceed 500 characters")]
    [NotifyDataErrorInfo]
    private string _updateDescription = string.Empty;

    [ObservableProperty]
    [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
    [NotifyDataErrorInfo]
    private string? _location;

    [ObservableProperty]
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    [NotifyDataErrorInfo]
    private string? _notes;

    public DeliveryUpdateViewModel()
    {
        ValidateAllProperties();
    }

    // Computed properties
    public string StatusDisplay => Status.ToString();

    public bool IsNewUpdate => DeliveryUpdateID == 0;

    public string UpdateTimeDisplay => UpdateDate.ToString("MMM dd, yyyy hh:mm tt");

    // Entity Mappers
    public static DeliveryUpdateViewModel FromEntity(DeliveryUpdate entity)
    {
        if (entity == null)
            return new DeliveryUpdateViewModel();

        return new DeliveryUpdateViewModel
        {
            DeliveryUpdateID = entity.DeliveryUpdateID,
            DeliveryID = entity.DeliveryID,
            UpdatedByUserID = entity.UpdatedByUserID,
            Status = entity.Status,
            UpdateDate = entity.UpdateDate,
            UpdateDescription = entity.UpdateDescription,
            Location = entity.Location,
            Notes = entity.Notes
        };
    }

    public DeliveryUpdate ToEntity()
    {
        return new DeliveryUpdate
        {
            DeliveryUpdateID = DeliveryUpdateID,
            DeliveryID = DeliveryID,
            UpdatedByUserID = UpdatedByUserID,
            Status = Status,
            UpdateDate = UpdateDate,
            UpdateDescription = UpdateDescription?.Trim() ?? string.Empty,
            Location = Location?.Trim(),
            Notes = Notes?.Trim(),
            UpdatedBy = null 
        };
    }

    // Validation helpers
    public string GetErrorMessages()
    {
        if (!HasErrors) return string.Empty;

        var errors = GetErrors()
            .Select(error => $"{error.MemberNames.FirstOrDefault()}: {error.ErrorMessage}");

        return string.Join(Environment.NewLine, errors);
    }

    // Helper method to create a status update
    public static DeliveryUpdateViewModel CreateStatusUpdate(int deliveryId, DeliveryStatus status, string description, int? updatedByUserId = null, string? location = null, string? notes = null)
    {
        return new DeliveryUpdateViewModel
        {
            DeliveryID = deliveryId,
            UpdatedByUserID = updatedByUserId,
            Status = status,
            UpdateDate = DateTime.Now,
            UpdateDescription = description,
            Location = location,
            Notes = notes
        };
    }

    // Reset method
    public void Reset()
    {
        DeliveryUpdateID = 0;
        DeliveryID = 0;
        UpdatedByUserID = null;
        Status = DeliveryStatus.Pending;
        UpdateDate = DateTime.Now;
        UpdateDescription = string.Empty;
        Location = null;
        Notes = null;

        ClearErrors();
    }
}
