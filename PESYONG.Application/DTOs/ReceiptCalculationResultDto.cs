using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.DTOs;

public class ReceiptCalculationResultDto
{
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal GrandTotal { get; set; }
}
