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
using Microsoft.IdentityModel.Tokens.Experimental;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.Domain.Entities.Users.Identity;
using PESYONG.Domain.Enums;
using Windows.Networking;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

public partial class AppUserViewModel : ObservableValidator
{
    private readonly AppUserRepository _userRepository;

    [ObservableProperty]
    private int? _id;

    [ObservableProperty]
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email is not a valid email address.")]
    private string _email = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Username is required.")]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string? _phoneNumber;

    [ObservableProperty]
    [Required(ErrorMessage = "First name is required.")]
    [MaxLength(64, ErrorMessage = "First name must be at most 64 characters.")]
    private string _firstName = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Last name is required.")]
    [MaxLength(64, ErrorMessage = "Last name must be at most 64 characters.")]
    private string _lastName = string.Empty;

    [ObservableProperty]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    private string? _password;

    [ObservableProperty]
    private AuthorizationType _authorizationType;

    [ObservableProperty]
    private bool _emailConfirmed;

    [ObservableProperty]
    private bool _phoneNumberConfirmed;

    [ObservableProperty]
    private bool _twoFactorEnabled;

    [ObservableProperty]
    private bool _lockoutEnabled;

    [ObservableProperty]
    private DateTimeOffset? _lockoutEnd;

    [ObservableProperty]
    private int _accessFailedCount;

    [ObservableProperty]
    private bool _hasValidationErrors;

    [ObservableProperty]
    private ObservableCollection<string> _validationErrors = new();

    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }

    public AppUserViewModel()
    {
        _userRepository = App.Instance.Services.GetRequiredService<AppUserRepository>();

        SaveCommand = new AsyncRelayCommand(SaveUserAsync, CanSaveUser);
        LoadCommand = new AsyncRelayCommand(LoadUserAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteUserAsync);

        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName != nameof(HasValidationErrors) &&
                e.PropertyName != nameof(ValidationErrors))
            {
                Validate();
                SaveCommand.NotifyCanExecuteChanged();
            }
        };
    }

    public bool CanSaveUser() => !HasValidationErrors;

    public static AppUserViewModel CreateFromEntity(AppUser user)
    {
        var vm = new AppUserViewModel();
        vm.LoadFromEntity(user);
        return vm;
    }

    public void LoadFromEntity(AppUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        Id = user.Id;
        Email = user.Email ?? string.Empty;
        UserName = user.UserName ?? string.Empty;
        PhoneNumber = user.PhoneNumber;
        FirstName = user.FirstName ?? string.Empty;
        LastName = user.LastName ?? string.Empty;
        AuthorizationType = user.AuthorizationType;
        EmailConfirmed = user.EmailConfirmed;
        PhoneNumberConfirmed = user.PhoneNumberConfirmed;
        TwoFactorEnabled = user.TwoFactorEnabled;
        LockoutEnabled = user.LockoutEnabled;
        LockoutEnd = user.LockoutEnd;
        AccessFailedCount = user.AccessFailedCount;
    }

    public AppUser ToEntity(string? passwordHash = null)
    {
        var user = new AppUser
        {
            Id = Id ?? 0,
            FirstName = FirstName,
            LastName = LastName,
            Email = Email,
            NormalizedEmail = NormalizeEmailForIdentity(Email),
            UserName = UserName,
            NormalizedUserName = NormalizeUserNameForIdentity(UserName),
            PhoneNumber = PhoneNumber,
            AuthorizationType = AuthorizationType,
            EmailConfirmed = EmailConfirmed,
            PhoneNumberConfirmed = PhoneNumberConfirmed,
            TwoFactorEnabled = TwoFactorEnabled,
            LockoutEnabled = LockoutEnabled,
            LockoutEnd = LockoutEnd,
            AccessFailedCount = AccessFailedCount
        };

        if (passwordHash != null)
        {
            user.PasswordHash = passwordHash;
        }

        return user;
    }

    private void Validate()
    {
        var errors = new List<string>();

        // Clear previous errors
        ValidationErrors.Clear();

        // Validate properties
        if (string.IsNullOrWhiteSpace(Email))
            errors.Add("Email is required");
        else if (!new EmailAddressAttribute().IsValid(Email))
            errors.Add("Email is not a valid email address");

        if (string.IsNullOrWhiteSpace(UserName))
            errors.Add("Username is required");

        if (string.IsNullOrWhiteSpace(FirstName))
            errors.Add("First name is required");
        else if (FirstName.Length > 64)
            errors.Add("First name must be at most 64 characters");

        if (string.IsNullOrWhiteSpace(LastName))
            errors.Add("Last name is required");
        else if (LastName.Length > 64)
            errors.Add("Last name must be at most 64 characters");

        if (!string.IsNullOrEmpty(Password) && Password.Length < 6)
            errors.Add("Password must be at least 6 characters");

        // Add validation errors to collection
        foreach (var error in errors)
        {
            ValidationErrors.Add(error);
        }

        HasValidationErrors = errors.Any();
    }

    private async Task SaveUserAsync()
    {
        if (!CanSaveUser() || _userRepository == null) return;

        try
        {
            if (Id.HasValue)
            {
                await _userRepository.UpdateUserAsync(ToEntity());
            }
            else
            {
                await _userRepository.CreateUserAsync(ToEntity(), Password);
            }
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", $"An error occurred while saving user: {ex.Message}", "OK");
        }
    }

    private async Task LoadUserAsync()
    {
        if (!Id.HasValue || _userRepository == null) return;

        try
        {
            var user = await _userRepository.GetUserByIdAsync(Id.Value);
            if (user != null)
            {
                LoadFromEntity(user);
            }
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", $"Failed to load user: {ex.Message}", "OK");
        }
    }

    private async Task DeleteUserAsync()
    {
        if (!Id.HasValue || _userRepository == null) return;

        try
        {
            await _userRepository.DeleteUserAsync(Id.Value);
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", $"An error occurred while deleting user: {ex.Message}", "OK");
        }
    }

    public void ClearAppUserViewModel()
    {
        Id = null;
        Email = string.Empty;
        UserName = string.Empty;
        PhoneNumber = null;
        FirstName = string.Empty;
        LastName = string.Empty;
        Password = null;
        AuthorizationType = default;
        EmailConfirmed = false;
        PhoneNumberConfirmed = false;
        TwoFactorEnabled = false;
        LockoutEnabled = false;
        LockoutEnd = null;
        AccessFailedCount = 0;
        ValidationErrors.Clear();
        HasValidationErrors = false;
    }

    private void ShowEventOnDebugConsole(string a, string b, string c)
    {
        Debug.Write($"[{a}] {c} : {b}");
    }

    // Additional properties for UI display
    public string FullName => $"{FirstName} {LastName}";
    public string AuthorizationTypeDisplay => AuthorizationType.ToString();
    public string StatusDisplay => LockoutEnabled && LockoutEnd > DateTimeOffset.Now ? "Locked" : "Active";
    public string EmailStatus => EmailConfirmed ? "Confirmed" : "Pending";
    public string PhoneStatus => PhoneNumberConfirmed ? "Confirmed" : "Pending";

    partial void OnFirstNameChanged(string value)
    {
        OnPropertyChanged(nameof(FullName));
    }

    partial void OnLastNameChanged(string value)
    {
        OnPropertyChanged(nameof(FullName));
    }

    partial void OnAuthorizationTypeChanged(AuthorizationType value)
    {
        OnPropertyChanged(nameof(AuthorizationTypeDisplay));
    }

    partial void OnLockoutEnabledChanged(bool value)
    {
        OnPropertyChanged(nameof(StatusDisplay));
    }

    partial void OnLockoutEndChanged(DateTimeOffset? value)
    {
        OnPropertyChanged(nameof(StatusDisplay));
    }

    partial void OnEmailConfirmedChanged(bool value)
    {
        OnPropertyChanged(nameof(EmailStatus));
    }

    partial void OnPhoneNumberConfirmedChanged(bool value)
    {
        OnPropertyChanged(nameof(PhoneStatus));
    }

    private static string NormalizeEmailForIdentity(string email) =>
        string.IsNullOrWhiteSpace(email) ? email : email.Normalize().ToUpperInvariant();

    private static string NormalizeUserNameForIdentity(string userName) =>
        string.IsNullOrWhiteSpace(userName) ? userName : userName.Normalize().ToUpperInvariant();
}
