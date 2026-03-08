using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens.Experimental;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.Domain.Entities.Users;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

/// <summary>
/// This is the customer view model for the Customer class.
/// </summary>
public partial class CustomerViewModel : ObservableValidator
{
    private readonly CustomerRepository _customerRepository;

    [ObservableProperty]
    private Guid customerID;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    [ObservableProperty]
    private string firstName = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    [ObservableProperty]
    private string lastName = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [ObservableProperty]
    private string email = string.Empty;

    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    [ObservableProperty]
    private string address = string.Empty;

    [ObservableProperty]
    private DateTime createdDate = DateTime.UtcNow;

    [ObservableProperty]
    private bool isActive = true;

    [ObservableProperty]
    private bool hasValidationErrors;

    [ObservableProperty]
    private ObservableCollection<string> validationErrors = new();

    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }

    public CustomerViewModel()
    {
        _customerRepository = App.Instance.Services.GetRequiredService<CustomerRepository>();

        SaveCommand = new AsyncRelayCommand(SaveCustomerAsync, CanSaveCustomer);
        LoadCommand = new AsyncRelayCommand(LoadCustomerAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteCustomerAsync);

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

    public static CustomerViewModel CreateFromEntity(Customer customer)
    {
        var vm = new CustomerViewModel();
        vm.LoadFromEntity(customer);
        return vm;
    }

    public void LoadFromEntity(Customer customer)
    {
        CustomerID = customer.CustomerID;
        FirstName = customer.FirstName;
        LastName = customer.LastName;
        Email = customer.Email;
        Address = customer.Address ?? string.Empty;
        CreatedDate = customer.CreatedDate;
        IsActive = customer.IsActive;
    }

    public Customer ToEntity()
    {
        return new Customer
        {
            CustomerID = CustomerID != Guid.Empty ? CustomerID : Guid.NewGuid(),
            FirstName = FirstName?.Trim() ?? string.Empty,
            LastName = LastName?.Trim() ?? string.Empty,
            Email = Email?.Trim() ?? string.Empty,
            Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
            CreatedDate = CreatedDate,
            IsActive = IsActive
        };
    }

    private bool CanSaveCustomer() => !HasValidationErrors;

    private bool CanDeleteCustomer() => CustomerID != Guid.Empty;

    public void ClearCustomerViewModel()
    {
        CustomerID = Guid.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        Address = string.Empty;
        CreatedDate = DateTime.UtcNow;
        IsActive = true;
    }

    private async Task SaveCustomerAsync()
    {
        if (!CanSaveCustomer() || _customerRepository == null) return;

        try
        {
            if (CustomerID != Guid.Empty)
            {
                await _customerRepository.UpdateCustomerAsync(ToEntity());
            }
            else
            {
                await _customerRepository.CreateCustomerAsync(ToEntity());
            }
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", $"An error occurred while saving customer: {ex.Message}", "OK");
        }
    }

    private async Task LoadCustomerAsync()
    {
        if (CustomerID == Guid.Empty || _customerRepository == null) return;

        try
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(CustomerID);
            if (customer != null)
            {
                LoadFromEntity(customer);
            }
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", $"Failed to load customer: {ex.Message}", "OK");
        }
    }

    private async Task DeleteCustomerAsync()
    {
        if (CustomerID == Guid.Empty || _customerRepository == null) return;

        try
        {
            await _customerRepository.DeleteCustomerAsync(CustomerID);
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", $"An error occurred while deleting customer: {ex.Message}", "OK");
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

    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string DisplayName => !string.IsNullOrWhiteSpace(FullName) ? FullName : "New Customer";
    public string StatusText => IsActive ? "Active" : "Inactive";
    public string IsActiveDisplay => IsActive ? "Yes" : "No";
    public bool IsNewCustomer => CustomerID == Guid.Empty;

    public string RelativeCreationTime
    {
        get
        {
            var span = DateTime.UtcNow - CreatedDate;

            if (span.TotalMinutes < 1) return "Just created";
            if (span.TotalHours < 1) return $"{(int)span.TotalMinutes} minute(s) ago";
            if (span.TotalDays < 1) return $"{(int)span.TotalHours} hour(s) ago";
            return $"{(int)span.TotalDays} day(s) ago";
        }
    }

    partial void OnFirstNameChanged(string value)
    {
        OnPropertyChanged(nameof(FullName));
        OnPropertyChanged(nameof(DisplayName));
    }

    partial void OnLastNameChanged(string value)
    {
        OnPropertyChanged(nameof(FullName));
        OnPropertyChanged(nameof(DisplayName));
    }

    partial void OnCreatedDateChanged(DateTime value)
    {
        OnPropertyChanged(nameof(RelativeCreationTime));
    }

    partial void OnIsActiveChanged(bool value)
    {
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(IsActiveDisplay));
    }
}
