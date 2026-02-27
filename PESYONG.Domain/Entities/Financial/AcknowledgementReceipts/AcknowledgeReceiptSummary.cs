using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.Domain.Entities.Financial.AcknowledgementReceipts;

/// <summary>
/// Helper class for AcknowledgementReciept, doesn't really compute anything.
/// TECH DEBT: This could've been a DTO instead.
/// </summary>

[NotMapped]
public class AcknowledgementReceiptSummary
{
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal GrandTotal { get; set; }
}
