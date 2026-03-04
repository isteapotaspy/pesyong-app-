using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PESYONG.Domain.Enums;

namespace PESYONG.ApplicationLogic.ViewModels;

// Main view model for displaying audit logs
public class AuditLogViewModel
{
    public int AuditLogID { get; set; }
    public int? UserID { get; set; }
    public string UserName { get; set; } = string.Empty;
    public AuditActionType AuditActionType { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityID { get; set; }
    public DateTime Timestamp { get; set; }
    public string TimestampDisplay => Timestamp.ToString("MM/dd/yyyy HH:mm:ss");
    public string? Changes { get; set; }
    public string? IPAddress { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    // Helper properties for UI
    public string ActionTypeName => AuditActionType.ToString();
    public string StatusIcon => Success ? "✓" : "✗";
}

// View model for creating audit logs
public class CreateAuditLogViewModel
{
    public AuditActionType AuditActionType { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityID { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? Changes { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Source { get; set; }
    public int? UserID { get; set; }
}

// View model for filtering audit logs
public class AuditLogFilterViewModel
{
    public AuditActionType? ActionType { get; set; }
    public string? EntityType { get; set; }
    public int? EntityID { get; set; }
    public int? UserID { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? Success { get; set; }

    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
