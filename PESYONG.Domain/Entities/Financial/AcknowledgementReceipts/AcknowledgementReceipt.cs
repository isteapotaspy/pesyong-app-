// In AcknowledgementReceipt.cs - fix the PaymentDate property
using PESYONG.Domain.Entities.Financial.Promos;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Entities.Users.Identity;
using PESYONG.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class AcknowledgementReceipt
{
    [Key]
    public int AcknowledgementReceiptID { get; private set; }

    [Required]
    public Guid OrderID { get; set; }

    [ForeignKey("OrderID")]
    public virtual Order? Order { get; private set; }

    [Required]
    public int CustomerID { get; set; }

    [ForeignKey("CustomerID")]
    public virtual AppUser? Customer { get; private set; }

    [ForeignKey("PromoID")]
    public virtual Promo? Promo { get; set; }

    // Receipt details
    [Required]
    public DateTime IssueDate { get; set; }

    // FIXED: Changed from private set to public set
    public DateTime? PaymentDate { get; set; }

    [Required]
    [StringLength(50)]
    public string ReceiptNumber { get; set; } = string.Empty;

    [Required]
    public PaymentStatus Status { get; set; }

    [NotMapped]
    public decimal Subtotal { get; set; }

    [NotMapped]
    public decimal DiscountAmount { get; set; }

    [NotMapped]
    public decimal TaxAmount { get; set; }

    [NotMapped]
    public decimal ShippingCost { get; set; }

    [NotMapped]
    public decimal GrandTotal { get; set; }

    public string Currency { get; set; } = "PHP";

    public AcknowledgementReceipt() {        
    
    }
}
