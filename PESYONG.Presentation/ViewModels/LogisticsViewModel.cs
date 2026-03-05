using PESYONG.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PESYONG.ApplicationLogic.ViewModels;

public class DeliveryViewModel
{
    public int DeliveryID { get; set; }
    public Guid OrderID { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int? DeliveryPersonnelID { get; set; }
    public string DeliveryPersonnelName { get; set; } = string.Empty;
    public DeliveryStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public DateTime CreatedDate { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;

    private string? _trackingNumber = string.Empty;
    public string? TrackingNumber
    {
        get => _trackingNumber;
        set 
        { }
    }
    public decimal ShippingCost { get; set; }
    public string? ShippingMethod { get; set; }
    public string? CarrierName { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? DeliveryNotes { get; set; }
    public string? CurrentLocation { get; set; }
    public DateTime? LastLocationUpdate { get; set; }
    public bool SignatureRequired { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public string? ReceivedBy { get; set; }
    public DateTime? ReceivedAt { get; set; }

    // Calculated properties
    public bool IsDelivered => ActualDeliveryDate.HasValue;
    public bool IsInTransit => Status == DeliveryStatus.InTransit;
    public string StatusColor => GetStatusColor();
    public string DeliveryTime => ActualDeliveryDate?.ToString("MM/dd/yyyy HH:mm") ?? "Not delivered";

    private string GetStatusColor()
    {
        return Status switch
        {
            DeliveryStatus.Delivered => "success",
            DeliveryStatus.InTransit => "warning",
            DeliveryStatus.Pending => "secondary",
            DeliveryStatus.Failed => "danger",
            _ => "info"
        };
    }
}

public class CreateDeliveryViewModel
{
    [Required]
    public Guid OrderID { get; set; }

    [Required]
    public string DeliveryAddress { get; set; } = string.Empty;

    public decimal ShippingCost { get; set; }
    public string? ShippingMethod { get; set; }
    public string? CarrierName { get; set; }
    public string? SpecialInstructions { get; set; }
    public bool SignatureRequired { get; set; } = true;
    public DateTime? EstimatedDeliveryDate { get; set; }
    public int? DeliveryPersonnelID { get; set; }
}

public class UpdateDeliveryStatusViewModel
{
    [Required]
    public int DeliveryID { get; set; }

    [Required]
    public DeliveryStatus NewStatus { get; set; }

    public string? UpdateDescription { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

public class DeliveryUpdateViewModel
{
    public int DeliveryUpdateID { get; set; }
    public int DeliveryID { get; set; }
    public int? UpdatedByUserID { get; set; }
    public string UpdatedByName { get; set; } = string.Empty;
    public DeliveryStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public DateTime UpdateDate { get; set; }
    public string UpdateDescription { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Notes { get; set; }

    public string UpdateTime => UpdateDate.ToString("MM/dd/yyyy HH:mm");
}

public class CreateDeliveryUpdateViewModel
{
    [Required]
    public int DeliveryID { get; set; }

    [Required]
    public DeliveryStatus Status { get; set; }

    [Required]
    public string UpdateDescription { get; set; } = string.Empty;

    public string? Location { get; set; }
    public string? Notes { get; set; }
    public int? UpdatedByUserID { get; set; }
}

public class LogisticsDashboardViewModel
{
    public int TotalDeliveries { get; set; }
    public int PendingDeliveries { get; set; }
    public int InTransitDeliveries { get; set; }
    public int DeliveredCount { get; set; }
    public int FailedDeliveries { get; set; }
    public List<DeliveryViewModel> RecentDeliveries { get; set; } = new();
    public List<DeliveryUpdateViewModel> RecentUpdates { get; set; } = new();
}
