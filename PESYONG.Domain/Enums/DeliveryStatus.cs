using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.Domain.Enums;

public enum DeliveryStatus
{
    OnCart,
    Pending,        // Order received, delivery not yet scheduled
    Confirmed,      // Order confirmed, awaiting scheduling
    Scheduled,      // Delivery scheduled for a specific date/time
    Preparing,      // Order being prepared for delivery
    ReadyForPickup, // Order ready for delivery personnel to pick up
    OutForDelivery, // Order picked up and out for delivery
    InTransit,      // Order in transit to destination
    Arrived,        // Delivery personnel arrived at location
    Attempted,      // Delivery attempted but unsuccessful
    Delivered,      // Successfully delivered
    Failed,         // Delivery failed (multiple attempts)
    Cancelled,      // Delivery cancelled
    Returned        // Order returned to sender
}

