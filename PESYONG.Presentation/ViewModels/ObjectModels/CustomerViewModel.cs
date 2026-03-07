using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PESYONG.Domain.Entities.Users;
using Windows.Networking;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

/// <summary>
/// This is the customer view model for the Customer class.
/// </summary>
public partial class CustomerViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    [NotifyPropertyChangedFor(nameof(IsNewCustomer))]
    private Guid _customerID;

    [ObservableProperty]
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(FullName))]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private string _firstName = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(FullName))]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private string _lastName = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private string _email = string.Empty;

    [ObservableProperty]
    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    [NotifyDataErrorInfo]
    private string _address = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(IsActiveDisplay))]
    private DateTime _createdDate = DateTime.UtcNow;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(IsActiveDisplay))]
    private bool _isActive = true;

    public CustomerViewModel()
    {
        // Validate all properties when changes occur
        PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != nameof(HasErrors) && args.PropertyName != nameof(CanSave))
            {
                OnPropertyChanged(nameof(CanSave));
            }
        };

        ValidateAllProperties();
    }

    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();

    public string DisplayName => !string.IsNullOrWhiteSpace(FullName) ? FullName : "New Customer";

    public string StatusText => IsActive ? "Active" : "Inactive";

    public string IsActiveDisplay => IsActive ? "Yes" : "No";

    public bool IsNewCustomer => CustomerID == Guid.Empty;

    public bool CanSave => !HasErrors && !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName) && !string.IsNullOrWhiteSpace(Email);

    // Entity Mappers
    public static CustomerViewModel FromEntity(Customer entity)
    {
        if (entity == null)
            return new CustomerViewModel();

        return new CustomerViewModel
        {
            CustomerID = entity.CustomerID,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            Address = entity.Address ?? string.Empty,
            CreatedDate = entity.CreatedDate,
            IsActive = entity.IsActive
        };
    }

    public Customer ToEntity()
    {
        return new Customer
        {
            CustomerID = CustomerID != Guid.Empty ? CustomerID : Guid.NewGuid(),
            FirstName = FirstName?.Trim() ?? string.Empty,
            LastName = LastName?.Trim() ?? string.Empty,
            Email = Email?.Trim() ?? string.Empty,
            Address = Address?.Trim() ?? string.Empty,
            CreatedDate = CreatedDate,
            IsActive = IsActive
        };
    }

    // Validation helpers
    public void ValidateAll()
    {
        ValidateAllProperties();
    }

    public string GetErrorMessages()
    {
        if (!HasErrors) return string.Empty;

        var errors = GetErrors()
            .Select(error => $"{error.MemberNames.FirstOrDefault()}: {error.ErrorMessage}");

        return string.Join(Environment.NewLine, errors);
    }

    public bool HasError(string propertyName)
    {
        return GetErrors(propertyName).Any();
    }

    public string GetError(string propertyName)
    {
        var errors = GetErrors(propertyName);
        return errors.FirstOrDefault()?.ErrorMessage ?? string.Empty;
    }

    // Custom validation methods
    public bool ValidateEmailUniqueness(Func<string, bool> uniquenessChecker)
    {
        if (uniquenessChecker == null) return true;

        bool isUnique = uniquenessChecker(Email);
        if (!isUnique)
        {
            //AddError(nameof(Email), "Email address is already in use");
            OnPropertyChanged(nameof(CanSave));
            return false;
        }

        ClearErrors(nameof(Email));
        OnPropertyChanged(nameof(CanSave));
        return true;
    }

    // Reset method
    public void Reset()
    {
        CustomerID = Guid.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        Address = string.Empty;
        CreatedDate = DateTime.UtcNow;
        IsActive = true;

        ClearErrors();
        ValidateAllProperties();
    }

    // Clone method
    public CustomerViewModel Clone()
    {
        return new CustomerViewModel
        {
            CustomerID = CustomerID,
            FirstName = FirstName,
            LastName = LastName,
            Email = Email,
            Address = Address,
            CreatedDate = CreatedDate,
            IsActive = IsActive
        };
    }

    // Method to check if viewmodel has changes compared to entity
    public bool HasChanges(Customer entity)
    {
        if (entity == null) return true;

        return FirstName != entity.FirstName ||
               LastName != entity.LastName ||
               Email != entity.Email ||
               Address != (entity.Address ?? string.Empty) ||
               IsActive != entity.IsActive;
    }
}
