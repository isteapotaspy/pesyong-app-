using PESYONG.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace PESYONG.ApplicationLogic.ViewModels;

public class AcknowledgementReceiptViewModel
{
    public int AcknowledgementReceiptID { get; set; }
    public Guid OrderID { get; set; }
    public int CustomerID { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal GrandTotal { get; set; }
}

public class CreateAcknowledgementReceiptViewModel
{
    [Required]
    public Guid OrderID { get; set; }
    [Required]
    public int CustomerID { get; set; }
    [Required]
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    [Required]
    public string ReceiptNumber { get; set; } = string.Empty;
}

public class AcknowledgementReceiptSummaryViewModel
{
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal GrandTotal { get; set; }
}

public class PromoViewModel
{
    public int PromoID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DiscountPercentageValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; } = 0;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
}

public class CreatePromoViewModel
{
    [Required]
    public string Code { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    public decimal DiscountPercentageValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public int? UsageLimit { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
}

public class PaymentViewModel
{
    public string PaymentID { get; set; } = string.Empty;
    public int AcknowledgementRecieptID { get; set; }
    public PaymentMethodType PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime TimeStamp { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class CreatePaymentViewModel
{
    [Required]
    public int AcknowledgementRecieptID { get; set; }
    [Required]
    public PaymentMethodType PaymentMethod { get; set; }
    [Required]
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}
