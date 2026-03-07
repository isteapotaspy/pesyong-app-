using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.ApplicationLogic.Services;
using PESYONG.Domain.Entities.Financial.Promos;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Entities.Users.Identity;
using PESYONG.Domain.Enums;

namespace PESYONG.Presentation.ViewModels.ObjectModels;

public partial class AcknowledgementReceiptViewModel : ObservableValidator
{
    private readonly AcknowledgementReceiptService _receiptService;

    [ObservableProperty]
    private int _acknowledgementReceiptID;

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

    public AcknowledgementReceiptViewModel()
    {
        _receiptService = App.Instance.Services
                .GetRequiredService<AcknowledgementReceiptService>();

        // Recalculate totals when any of the calculation properties change
        PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(Subtotal) ||
                args.PropertyName == nameof(DiscountPercentage) ||
                args.PropertyName == nameof(TaxPercentage) ||
                args.PropertyName == nameof(ShippingCost))
            {
                CalculateTotals();
            }
        };
    }

    private void CalculateTotals()
    {
        if (HasErrors) return;

        try
        {
            var result = _receiptService.CalculateTotals(Subtotal, ShippingCost);
            Subtotal = result.Subtotal;
            ShippingCost = result.ShippingCost;
            GrandTotal = result.GrandTotal;
        }
        catch (ArgumentException)
        {
            // Clear calculated fields if input is invalid
            DiscountAmount = 0;
            TaxAmount = 0;
            GrandTotal = 0;
        }
    }

    public static ValidationResult? ValidateReceiptNumber(string receiptNumber, ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(receiptNumber))
            return new ValidationResult("Receipt number is required");

        if (receiptNumber.Length > 50)
            return new ValidationResult("Receipt number cannot exceed 50 characters");

        return ValidationResult.Success;
    }

    // Entity Mappers
    public static AcknowledgementReceiptViewModel FromEntity(AcknowledgementReceipt entity)
    {
        var viewModel = new AcknowledgementReceiptViewModel()
        {
            AcknowledgementReceiptID = entity.AcknowledgementReceiptID,
            OrderID = entity.OrderID,
            Order = entity.Order,
            CustomerID = entity.CustomerID,
            Customer = entity.Customer,
            Promo = entity.Promo,
            PromoID = entity.Promo?.PromoID,
            IssueDate = entity.IssueDate,
            PaymentDate = entity.PaymentDate,
            ReceiptNumber = entity.ReceiptNumber,
            Status = entity.Status,
            Subtotal = entity.Subtotal,
            DiscountAmount = entity.DiscountAmount,
            TaxAmount = entity.TaxAmount,
            ShippingCost = entity.ShippingCost,
            GrandTotal = entity.GrandTotal,
            Currency = entity.Currency
        };

        // Calculate discount percentage for the view model
        if (entity.Subtotal > 0)
        {
            viewModel.DiscountPercentage = (entity.DiscountAmount / entity.Subtotal) * 100;
            viewModel.TaxPercentage = (entity.TaxAmount / (entity.Subtotal - entity.DiscountAmount)) * 100;
        }

        return viewModel;
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
}