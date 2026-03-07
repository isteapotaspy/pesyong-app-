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
using PESYONG.ApplicationLogic.Services;
using PESYONG.Domain.Entities.Financial.Promos;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Entities.Users.Identity;
using PESYONG.Domain.Enums;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

public partial class AcknowledgementReceiptViewModel : ObservableValidator
{
    private readonly AcknowledgementReceiptService _receiptService;
    private readonly AcknowledgementReceiptRepository _acknowledgementReceiptRepository;

    [ObservableProperty]
    private int? _acknowledgementReceiptID;

    [ObservableProperty]
    [Required]
    private Guid _orderID;

    [ObservableProperty]
    private Order? _order;

    [ObservableProperty]
    [Required]
    private int _customerID;

    [ObservableProperty]
    private AppUser? _customer;

    [ObservableProperty]
    private Promo? _promo;

    [ObservableProperty]
    private int? _promoID;

    [ObservableProperty]
    [Required]
    private DateTime _issueDate = DateTime.Now;

    [ObservableProperty]
    private DateTime? _paymentDate;

    [ObservableProperty]
    [Required]
    [CustomValidation(typeof(AcknowledgementReceiptViewModel), nameof(ValidateReceiptNumber))]
    private string _receiptNumber = string.Empty;

    [ObservableProperty]
    [Required]
    private PaymentStatus _status;

    [ObservableProperty]
    [Range(0, double.MaxValue, ErrorMessage = "Subtotal must be non-negative")]
    private decimal _subtotal;

    [ObservableProperty]
    [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100")]
    private decimal _discountPercentage;

    [ObservableProperty]
    [Range(0, double.MaxValue, ErrorMessage = "Tax percentage must be non-negative")]
    private decimal _taxPercentage;

    [ObservableProperty]
    [Range(0, double.MaxValue, ErrorMessage = "Shipping cost must be non-negative")]
    private decimal _shippingCost;

    [ObservableProperty]
    private decimal _discountAmount;

    [ObservableProperty]
    private decimal _taxAmount;

    [ObservableProperty]
    private decimal _grandTotal;

    [ObservableProperty]
    private string _currency = "PHP";

    [ObservableProperty]
    private bool _hasValidationErrors;

    [ObservableProperty]
    private ObservableCollection<string> _validationErrors = new();

    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }

    public AcknowledgementReceiptViewModel()
    {
        _receiptService = App.Instance.Services
            .GetRequiredService<AcknowledgementReceiptService>();

        _acknowledgementReceiptRepository = App.Instance.Services
            .GetRequiredService<AcknowledgementReceiptRepository>();

        SaveCommand = new AsyncRelayCommand(SaveAcknowledgementReceiptAsync, CanSaveAcknowledgementReceipt);
        LoadCommand = new AsyncRelayCommand(LoadAcknowledgementReceiptAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAcknowledgementReceiptAsync);

        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName != nameof(HasValidationErrors) &&
                e.PropertyName != nameof(ValidationErrors))
            {
                Validate();
                SaveCommand.NotifyCanExecuteChanged();
            }

            if (e.PropertyName == nameof(Subtotal) ||
                e.PropertyName == nameof(DiscountPercentage) ||
                e.PropertyName == nameof(TaxPercentage) ||
                e.PropertyName == nameof(ShippingCost))
            {
                CalculateTotals();
            }
        };
    }

    public bool CanSaveAcknowledgementReceipt() => !HasValidationErrors;

    //partial void OnSubtotalChanged(decimal value) => CalculateTotals();
    //partial void OnDiscountPercentageChanged(decimal value) => CalculateTotals();
    //partial void OnTaxPercentageChanged(decimal value) => CalculateTotals();
    //partial void OnShippingCostChanged(decimal value) => CalculateTotals();

    private void CalculateTotals()
    {
        if (HasValidationErrors) return;

        try
        {
            DiscountAmount = Subtotal * (DiscountPercentage / 100);
            var taxableAmount = Subtotal - DiscountAmount;
            TaxAmount = taxableAmount * (TaxPercentage / 100);
            GrandTotal = taxableAmount + TaxAmount + ShippingCost;
        }
        catch (ArgumentException)
        {
            // Clear calculated fields if input is invalid
            DiscountAmount = 0;
            TaxAmount = 0;
            GrandTotal = 0;
        }
    }

    public static AcknowledgementReceiptViewModel CreateFromEntity(AcknowledgementReceipt entity)
    {
        var vm = new AcknowledgementReceiptViewModel();
        vm.LoadFromEntity(entity);
        return vm;
    }

    public void LoadFromEntity(AcknowledgementReceipt entity)
    {
        AcknowledgementReceiptID = entity.AcknowledgementReceiptID;
        OrderID = entity.OrderID;
        Order = entity.Order;
        CustomerID = entity.CustomerID;
        Customer = entity.Customer;
        Promo = entity.Promo;
        PromoID = entity.Promo?.PromoID;
        IssueDate = entity.IssueDate;
        PaymentDate = entity.PaymentDate;
        ReceiptNumber = entity.ReceiptNumber;
        Status = entity.Status;
        Subtotal = entity.Subtotal;

        // Calculate discount percentage for the view model
        if (entity.Subtotal > 0)
        {
            DiscountPercentage = (entity.DiscountAmount / entity.Subtotal) * 100;
            TaxPercentage = (entity.TaxAmount / (entity.Subtotal - entity.DiscountAmount)) * 100;
        }
        else
        {
            DiscountPercentage = 0;
            TaxPercentage = 0;
        }

        ShippingCost = entity.ShippingCost;
        DiscountAmount = entity.DiscountAmount;
        TaxAmount = entity.TaxAmount;
        GrandTotal = entity.GrandTotal;
        Currency = entity.Currency;
    }

    public AcknowledgementReceipt ToEntity()
    {
        return new AcknowledgementReceipt
        {
            OrderID = OrderID,
            CustomerID = CustomerID,
            Promo = Promo,
            IssueDate = IssueDate,
            PaymentDate = PaymentDate,
            ReceiptNumber = ReceiptNumber,
            Status = Status,
            Subtotal = Subtotal,
            DiscountAmount = DiscountAmount,
            TaxAmount = TaxAmount,
            ShippingCost = ShippingCost,
            GrandTotal = GrandTotal,
            Currency = Currency
        };
    }

    public static ValidationResult? ValidateReceiptNumber(string receiptNumber, ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(receiptNumber))
            return new ValidationResult("Receipt number is required");

        if (receiptNumber.Length > 50)
            return new ValidationResult("Receipt number cannot exceed 50 characters");

        return ValidationResult.Success;
    }

    private void Validate()
    {
        var errors = new List<string>();

        // Clear previous errors
        ValidationErrors.Clear();

        // Validate properties
        if (string.IsNullOrWhiteSpace(ReceiptNumber))
            errors.Add("Receipt number is required");
        else if (ReceiptNumber.Length > 50)
            errors.Add("Receipt number cannot exceed 50 characters");

        if (Subtotal < 0)
            errors.Add("Subtotal must be non-negative");

        if (DiscountPercentage < 0 || DiscountPercentage > 100)
            errors.Add("Discount percentage must be between 0 and 100");

        if (TaxPercentage < 0)
            errors.Add("Tax percentage must be non-negative");

        if (ShippingCost < 0)
            errors.Add("Shipping cost must be non-negative");

        // Add validation errors to collection
        foreach (var error in errors)
        {
            ValidationErrors.Add(error);
        }

        HasValidationErrors = errors.Any();
    }

    private async Task SaveAcknowledgementReceiptAsync()
    {
        if (!CanSaveAcknowledgementReceipt() || _acknowledgementReceiptRepository == null) return;

        try
        {
            if (AcknowledgementReceiptID.HasValue)
            {
                await _acknowledgementReceiptRepository.UpdateAcknowledgementReceiptAsync(ToEntity());
            }
            else
            {
                await _acknowledgementReceiptRepository.CreateAcknowledgementReceiptAsync(ToEntity());
            }
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", $"An error occurred while saving acknowledgement receipt: {ex.Message}", "OK");
        }
    }

    private async Task LoadAcknowledgementReceiptAsync()
    {
        if (!AcknowledgementReceiptID.HasValue || _acknowledgementReceiptRepository == null) return;

        try
        {
            var acknowledgementReceipt = await _acknowledgementReceiptRepository.GetAcknowledgementReceiptByIdAsync(AcknowledgementReceiptID.Value);
            if (acknowledgementReceipt != null)
            {
                LoadFromEntity(acknowledgementReceipt);
            }
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", $"Failed to load acknowledgement receipt: {ex.Message}", "OK");
        }
    }

    private async Task DeleteAcknowledgementReceiptAsync()
    {
        if (!AcknowledgementReceiptID.HasValue || _acknowledgementReceiptRepository == null) return;

        try
        {
            await _acknowledgementReceiptRepository.DeleteAcknowledgementReceiptAsync(AcknowledgementReceiptID.Value);
        }
        catch (Exception ex)
        {
            ShowEventOnDebugConsole("Error", $"An error occurred while deleting acknowledgement receipt: {ex.Message}", "OK");
        }
    }

    public void ClearAcknowledgementReceiptViewModel()
    {
        AcknowledgementReceiptID = null;
        OrderID = Guid.Empty;
        Order = null;
        CustomerID = 0;
        Customer = null;
        Promo = null;
        PromoID = null;
        IssueDate = DateTime.Now;
        PaymentDate = null;
        ReceiptNumber = string.Empty;
        Status = default;
        Subtotal = 0;
        DiscountPercentage = 0;
        TaxPercentage = 0;
        ShippingCost = 0;
        DiscountAmount = 0;
        TaxAmount = 0;
        GrandTotal = 0;
        Currency = "PHP";
        ValidationErrors.Clear();
        HasValidationErrors = false;
    }

    private void ShowEventOnDebugConsole(string a, string b, string c)
    {
        Debug.Write($"[{a}] {c} : {b}");
    }

    // Additional properties for UI display
    public string FormattedSubtotal => $"{Currency} {Subtotal:N2}";
    public string FormattedDiscountAmount => $"{Currency} {DiscountAmount:N2}";
    public string FormattedTaxAmount => $"{Currency} {TaxAmount:N2}";
    public string FormattedShippingCost => $"{Currency} {ShippingCost:N2}";
    public string FormattedGrandTotal => $"{Currency} {GrandTotal:N2}";

    public string StatusDisplay => Status.ToString();

    public string PaymentDateDisplay => PaymentDate?.ToString("MM/dd/yyyy") ?? "Not paid";

    //partial void OnSubtotalChanged(decimal value)
    //{
    //    OnPropertyChanged(nameof(FormattedSubtotal));
    //    OnPropertyChanged(nameof(FormattedGrandTotal));
    //}

    //partial void OnDiscountAmountChanged(decimal value)
    //{
    //    OnPropertyChanged(nameof(FormattedDiscountAmount));
    //    OnPropertyChanged(nameof(FormattedGrandTotal));
    //}

    //partial void OnTaxAmountChanged(decimal value)
    //{
    //    OnPropertyChanged(nameof(FormattedTaxAmount));
    //    OnPropertyChanged(nameof(FormattedGrandTotal));
    //}

    //partial void OnShippingCostChanged(decimal value)
    //{
    //    OnPropertyChanged(nameof(FormattedShippingCost));
    //    OnPropertyChanged(nameof(FormattedGrandTotal));
    //}

    //partial void OnGrandTotalChanged(decimal value)
    //{
    //    OnPropertyChanged(nameof(FormattedGrandTotal));
    //}

    partial void OnStatusChanged(PaymentStatus value)
    {
        OnPropertyChanged(nameof(StatusDisplay));
    }

    partial void OnPaymentDateChanged(DateTime? value)
    {
        OnPropertyChanged(nameof(PaymentDateDisplay));
    }

    partial void OnCurrencyChanged(string value)
    {
        OnPropertyChanged(nameof(FormattedSubtotal));
        OnPropertyChanged(nameof(FormattedDiscountAmount));
        OnPropertyChanged(nameof(FormattedTaxAmount));
        OnPropertyChanged(nameof(FormattedShippingCost));
        OnPropertyChanged(nameof(FormattedGrandTotal));
    }
}
