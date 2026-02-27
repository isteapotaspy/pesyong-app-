using PESYONG.Domain.Entities.Financial.Promos;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Entities.Users.Identity;
using PESYONG.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.Domain.Entities.Financial.AcknowledgementReceipts;

/// <summary>
/// This is NOT a final receipt. This just recognizes that an order is checked out,
/// and an order is on the way or reserved already. 
/// 
/// TECH DEBT: Transition this Rich Domain Model into an Anemic Model later on.
/// </summary>

public class AcknowledgementReceipt
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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

    public DateTime? PaymentDate { get; private set; }

    [Required]
    [StringLength(50)]
    public string ReceiptNumber { get; set; } = string.Empty;

    [Required]
    public PaymentStatus Status { get; set; }

    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal GrandTotal { get; set; }
    public string Currency { get; set; } = "PHP";

    /*
    [Column(TypeName = "nvarchar(MAX)")]
    [StringLength(100)]
    public string Notes { get; private set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; private set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; private set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; private set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ShippingCost { get; private set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal GrandTotal { get; private set; }

    [NotMapped]
    public decimal TaxableAmount => Subtotal - DiscountAmount;

    [Required]
    [StringLength(50)]
    public string PaymentMethod { get; private set; } = string.Empty;

    [StringLength(100)]
    public string PaymentTransactionID { get; private set; } = string.Empty;

    [Required]
    [StringLength(3)]
    public string Currency { get; private set; } = "PHP";
    */

    public AcknowledgementReceipt() { }

}