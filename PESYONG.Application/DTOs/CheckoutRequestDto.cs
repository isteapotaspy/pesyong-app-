using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.DTOs
{
    public class CheckoutRequestDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }

        public string Address { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public double? Distance { get; set; }
        public double DeliveryFee { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? DeliveryDateTime { get; set; }

        public string? Notes { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;

        public List<CheckoutItemDto> Items { get; set; } = new();
    }
}
