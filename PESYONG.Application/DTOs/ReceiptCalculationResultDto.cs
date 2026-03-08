using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.DTOs;

/// <summary>
/// Represents the computed receipt totals for an order.
/// </summary>
public class ReceiptCalculationResultDto
{
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal GrandTotal { get; set; }
}
