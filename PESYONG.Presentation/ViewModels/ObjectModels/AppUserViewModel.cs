using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PESYONG.Domain.Entities.Users.Identity;
using PESYONG.Domain.Enums;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

public partial class AppUserViewModel : ObservableValidator
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = $"{nameof(Email)} is required.")]
    [EmailAddress(ErrorMessage = $"{nameof(Email)} is not a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string UserName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = $"{nameof(FirstName)} is required.")]
    [MaxLength(64, ErrorMessage = $"{nameof(FirstName)} must be at most 64 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = $"{nameof(LastName)} is required.")]
    [MaxLength(64, ErrorMessage = $"{nameof(LastName)} must be at most 64 characters.")]
    public string LastName { get; set; } = string.Empty;

    [MinLength(6, ErrorMessage = $"{nameof(Password)} must be at least 6 characters.")]
    public string? Password { get; set; }

    public AuthorizationType AuthorizationType { get; set; }

    // Idk if we should expose this to UI
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }

    // Mapping helpers
    public void ToEntity(AppUser user, string? passwordHash = null)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.FirstName = FirstName;
        user.LastName = LastName;

        // Identity core fields
        user.Email = Email;
        user.NormalizedEmail = NormalizeEmailForIdentity(Email);
        user.UserName = UserName;
        user.NormalizedUserName = NormalizeUserNameForIdentity(UserName);
        user.PhoneNumber = PhoneNumber;

        user.EmailConfirmed = EmailConfirmed;
        user.PhoneNumberConfirmed = PhoneNumberConfirmed;
        user.TwoFactorEnabled = TwoFactorEnabled;
        user.LockoutEnabled = LockoutEnabled;
        user.LockoutEnd = LockoutEnd;
        user.AccessFailedCount = AccessFailedCount;

        if (passwordHash != null)
        {
            user.PasswordHash = passwordHash;
        }
    }

    public static AppUserViewModel FromEntity(AppUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new AppUserViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            AuthorizationType = user.AuthorizationType,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            LockoutEnabled = user.LockoutEnabled,
            LockoutEnd = user.LockoutEnd,
            AccessFailedCount = user.AccessFailedCount
        };
    }

    private static string NormalizeEmailForIdentity(string email) =>
        string.IsNullOrWhiteSpace(email) ? email : email.Normalize().ToUpperInvariant();

    private static string NormalizeUserNameForIdentity(string userName) =>
        string.IsNullOrWhiteSpace(userName) ? userName : userName.Normalize().ToUpperInvariant();
}