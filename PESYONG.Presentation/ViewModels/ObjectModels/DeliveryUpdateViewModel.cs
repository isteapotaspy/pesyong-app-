using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.IdentityModel.Tokens.Experimental;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.Domain.Entities.Logistics;
using PESYONG.Domain.Enums;
using PESYONG.Presentation;
using static Azure.Core.HttpHeader;

public partial class DeliveryUpdateViewModel : ObservableValidator
{
    private DeliveryUpdateRepository _deliveryUpdateRepository;

    [ObservableProperty]
    private int deliveryUpdateID;

    [ObservableProperty]
    private int deliveryID;

    [ObservableProperty]
    private int? updatedByUserID;

    [ObservableProperty]
    private DeliveryStatus status;

    [ObservableProperty]
    private DateTime updateDate = DateTime.Now;

    [ObservableProperty]
    private string updateDescription = string.Empty;

    [ObservableProperty]
    private string? location;

    [ObservableProperty]
    private string? notes;

    [ObservableProperty]
    private bool hasValidationErrors;

    [ObservableProperty]
    private ObservableCollection<string> validationErrors = new();

    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }

    public DeliveryUpdateViewModel()
    {
        SaveCommand = new AsyncRelayCommand(SaveDeliveryUpdateAsync, CanSaveDeliveryUpdate);
        LoadCommand = new AsyncRelayCommand(LoadDeliveryUpdateAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteDeliveryUpdateAsync);

        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName != nameof(HasValidationErrors) &&
                e.PropertyName != nameof(ValidationErrors))
            {
                Validate();
                SaveCommand.NotifyCanExecuteChanged();
                DeleteCommand.NotifyCanExecuteChanged();
            }
        };
    }

    public static DeliveryUpdateViewModel CreateFromEntity(DeliveryUpdate deliveryUpdate)
    {
        var vm = new DeliveryUpdateViewModel();
        vm.LoadFromEntity(deliveryUpdate);
        return vm;
    }

    public void LoadFromEntity(DeliveryUpdate deliveryUpdate)
    {
        DeliveryUpdateID = deliveryUpdate.DeliveryUpdateID;
        DeliveryID = deliveryUpdate.DeliveryID;
        UpdatedByUserID = deliveryUpdate.UpdatedByUserID;
        Status = deliveryUpdate.Status;
        UpdateDate = deliveryUpdate.UpdateDate;
        UpdateDescription = deliveryUpdate.UpdateDescription ?? string.Empty;
        Location = deliveryUpdate.Location;
        Notes = deliveryUpdate.Notes;
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
            UpdateDescription = string.IsNullOrWhiteSpace(UpdateDescription) ? null : UpdateDescription.Trim(),
            Location = string.IsNullOrWhiteSpace(Location) ? null : Location.Trim(),
            Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim()
        };
    }

    private bool CanSaveDeliveryUpdate() => !HasValidationErrors;

    private bool CanDeleteDeliveryUpdate() => DeliveryUpdateID != 0;

    public void ClearDeliveryUpdateViewModel()
    {
        DeliveryUpdateID = 0;
        DeliveryID = 0;
        UpdatedByUserID = null;
        Status = DeliveryStatus.Pending;
        UpdateDate = DateTime.Now;
        UpdateDescription = string.Empty;
        Location = null;
        Notes = null;
    }

    private async Task SaveDeliveryUpdateAsync()
    {
        if (!CanSaveDeliveryUpdate() || _deliveryUpdateRepository == null) return;

        try
        {
            if (DeliveryUpdateID != 0)
            {
                await _deliveryUpdateRepository.UpdateDeliveryUpdateAsync(ToEntity());
            }
            else
            {
                await _deliveryUpdateRepository.CreateDeliveryUpdateAsync(ToEntity());
            }
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", $"An error occurred while saving delivery update: {ex.Message}", "OK");
        }
    }

    private async Task LoadDeliveryUpdateAsync()
    {
        if (DeliveryUpdateID == 0 || _deliveryUpdateRepository == null) return;

        try
        {
            var deliveryUpdate = await _deliveryUpdateRepository.GetDeliveryUpdateByIdAsync(DeliveryUpdateID);
            if (deliveryUpdate != null)
            {
                LoadFromEntity(deliveryUpdate);
            }
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", $"Failed to load delivery update: {ex.Message}", "OK");
        }
    }

    private async Task DeleteDeliveryUpdateAsync()
    {
        if (DeliveryUpdateID == 0 || _deliveryUpdateRepository == null) return;

        try
        {
            await _deliveryUpdateRepository.DeleteDeliveryUpdateAsync(DeliveryUpdateID);
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", $"An error occurred while deleting delivery update: {ex.Message}", "OK");
        }
    }

    private void Validate()
    {
        var entity = ToEntity();
        var errors = entity.ValidationErrors.ToList();

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

    // Computed properties
    public string StatusDisplay => Status.ToString();

    public bool IsNewUpdate => DeliveryUpdateID == 0;

    public string UpdateTimeDisplay => UpdateDate.ToString("MMM dd, yyyy hh:mm tt");

    public string RelativeUpdateTime
    {
        get
        {
            var span = DateTime.Now - UpdateDate;

            if (span.TotalMinutes < 1) return "Just now";
            if (span.TotalHours < 1) return $"{(int)span.TotalMinutes} minute(s) ago";
            if (span.TotalDays < 1) return $"{(int)span.TotalHours} hour(s) ago";
            return $"{(int)span.TotalDays} day(s) ago";
        }
    }

    public string LocationDisplay => string.IsNullOrWhiteSpace(Location) ? "No location" : Location;

    public string NotesSummary => string.IsNullOrWhiteSpace(Notes) ? "No notes" :
        Notes.Length > 50 ? Notes.Substring(0, 47) + "..." : Notes;

    partial void OnUpdateDateChanged(DateTime value)
    {
        OnPropertyChanged(nameof(UpdateTimeDisplay));
        OnPropertyChanged(nameof(RelativeUpdateTime));
    }

    partial void OnStatusChanged(DeliveryStatus value)
    {
        OnPropertyChanged(nameof(StatusDisplay));
    }

    partial void OnLocationChanged(string? value)
    {
        OnPropertyChanged(nameof(LocationDisplay));
    }

    partial void OnNotesChanged(string? value)
    {
        OnPropertyChanged(nameof(NotesSummary));
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
}
