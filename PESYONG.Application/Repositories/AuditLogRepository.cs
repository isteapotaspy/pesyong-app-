using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Entities.Audits;
using PESYONG.Domain.Enums;
using PESYONG.Infrastructure;

namespace PESYONG.ApplicationLogic.Repositories;

public class AuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    // CREATE. To log a new audit entry in the database.
    public async Task CreateAuditLogAsync(AuditLog auditLog)
    {
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    // Get a specific audit log by its ID
    public async Task<AuditLog> GetAuditLogByIdAsync(int id)
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.AuditLogID == id);
    }

    // Get all audit logs
    public async Task<List<AuditLog>> GetAllAuditLogsAsync()
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    // Advanced querying using IQueryable for complex filtering
    public async Task<List<AuditLog>> GetAuditLogsAsync(IQueryable<AuditLog> query)
    {
        return await query.ToListAsync();
    }

    // Get audit logs by user ID - FIXED
    public async Task<List<AuditLog>> GetAuditLogsByUserIdAsync(int? userId)
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .Where(a => !userId.HasValue || a.UserID == userId.Value)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    // Get audit logs by entity type
    public async Task<List<AuditLog>> GetAuditLogsByEntityTypeAsync(string entityType)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityType == entityType)
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    // Get audit logs by entity ID and type
    public async Task<List<AuditLog>> GetAuditLogsByEntityAsync(string entityType, int entityId)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityID == entityId)
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    // Get audit logs by action type
    public async Task<List<AuditLog>> GetAuditLogsByActionTypeAsync(AuditActionType actionType)
    {
        return await _context.AuditLogs
            .Where(a => a.AuditActionType == actionType)
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    // Get audit logs within a date range
    public async Task<List<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.AuditLogs
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    // Get failed audit logs
    public async Task<List<AuditLog>> GetFailedAuditLogsAsync()
    {
        return await _context.AuditLogs
            .Where(a => a.Success == false)
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    // Get recent audit logs with limit
    public async Task<List<AuditLog>> GetRecentAuditLogsAsync(int limit = 50)
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    // Get audit statistics
    public async Task<AuditStatistics> GetAuditStatisticsAsync()
    {
        var totalLogs = await _context.AuditLogs.CountAsync();
        var successfulLogs = await _context.AuditLogs.CountAsync(a => a.Success);
        var failedLogs = totalLogs - successfulLogs;
        var todayLogs = await _context.AuditLogs.CountAsync(a => a.Timestamp.Date == DateTime.UtcNow.Date);

        var actionCounts = await _context.AuditLogs
            .GroupBy(a => a.AuditActionType)
            .Select(g => new { Action = g.Key, Count = g.Count() })
            .ToListAsync();

        return new AuditStatistics
        {
            TotalLogs = totalLogs,
            SuccessfulLogs = successfulLogs,
            FailedLogs = failedLogs,
            TodayLogs = todayLogs,
            ActionCounts = actionCounts.ToDictionary(x => x.Action, x => x.Count)
        };
    }

    // Clean up old audit logs
    public async Task<int> CleanOldAuditLogsAsync(DateTime olderThan)
    {
        var oldLogs = _context.AuditLogs.Where(a => a.Timestamp < olderThan);
        var count = await oldLogs.CountAsync();
        _context.AuditLogs.RemoveRange(oldLogs);
        await _context.SaveChangesAsync();
        return count;
    }

    // Update audit log (typically used to mark as failed)
    public async Task UpdateAuditLogAsync(AuditLog auditLog)
    {
        _context.AuditLogs.Update(auditLog);
        await _context.SaveChangesAsync();
    }

    // Delete audit log by ID
    public async Task DeleteAuditLogAsync(int auditLogId)
    {
        var auditLog = await _context.AuditLogs.FindAsync(auditLogId);
        if (auditLog != null)
        {
            _context.AuditLogs.Remove(auditLog);
            await _context.SaveChangesAsync();
        }
    }

    // Delete audit log by object
    public async Task DeleteAuditLogAsync(AuditLog auditLog)
    {
        _context.AuditLogs.Remove(auditLog);
        await _context.SaveChangesAsync();
    }
}

// Simple statistics class to hold audit data
public class AuditStatistics
{
    public int TotalLogs { get; set; }
    public int SuccessfulLogs { get; set; }
    public int FailedLogs { get; set; }
    public int TodayLogs { get; set; }
    public Dictionary<AuditActionType, int> ActionCounts { get; set; } = new();
}
