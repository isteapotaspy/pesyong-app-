using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PESYONG.ApplicationLogic.DTOs;

namespace PESYONG.ApplicationLogic.Services;

public class AcknowledgementReceiptService
{
    private decimal _taxPercentage = 12m;
    private decimal _discountPercentage = 0m;

    public ReceiptCalculationResultDto CalculateTotals(decimal subtotal, decimal shippingCost)
    {
        if (subtotal < 0) throw new 
                    ArgumentException("Subtotal cannot be negative");

        if (_discountPercentage < 0 || _discountPercentage > 100) throw new 
                    ArgumentException("Discount percentage must be between 0 and 100");

        if (_taxPercentage < 0) throw new 
                    ArgumentException("Tax percentage cannot be negative");

        if (shippingCost < 0) throw new 
                    ArgumentException("Shipping cost cannot be negative");

        var discountAmount = subtotal * (_discountPercentage / 100m);
        var taxableAmount = subtotal - discountAmount;
        var taxAmount = taxableAmount * (_taxPercentage / 100m);
        var grandTotal = taxableAmount + taxAmount + shippingCost;


        return new ReceiptCalculationResultDto
        {
            Subtotal = subtotal,
            ShippingCost = shippingCost,
            GrandTotal = grandTotal
        };
    }
}