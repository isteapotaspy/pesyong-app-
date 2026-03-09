using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Entities.Logistics;
using PESYONG.Domain.Enums;
using PESYONG.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.Repositories;

/// <summary>
/// Provides data access operations for delivery updates, including creation,
/// retrieval, filtering, modification, and deletion.
/// </summary>
public class DeliveryUpdateRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeliveryUpdateRepository"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public DeliveryUpdateRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new delivery update record in the database.
    /// </summary>
    /// <param name="deliveryUpdate">The delivery update to create.</param>
    public async Task CreateDeliveryUpdateAsync(DeliveryUpdate deliveryUpdate)
    {
        _context.DeliveryUpdates.Add(deliveryUpdate);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves a delivery update by its ID, including related delivery
    /// and user information.
    /// </summary>
    /// <param name="id">The ID of the delivery update.</param>
    /// <returns>The matching delivery update, if found.</returns>
    public async Task<DeliveryUpdate?> GetDeliveryUpdateByIdAsync(int id)
    {
        return await _context.DeliveryUpdates
            .Include(du => du.Delivery)
            .Include(du => du.UpdatedByUser)
            .FirstOrDefaultAsync(du => du.DeliveryUpdateID == id);
    }

    /// <summary>
    /// Retrieves all delivery updates ordered by most recent update date.
    /// </summary>
    /// <returns>A list of all delivery updates.</returns>
    public async Task<List<DeliveryUpdate>> GetAllDeliveryUpdatesAsync()
    {
        return await _context.DeliveryUpdates
            .Include(du => du.Delivery)
            .Include(du => du.UpdatedByUser)
            .OrderByDescending(du => du.UpdateDate)
            .ToListAsync();
    }

    /// <summary>
    /// Executes a custom delivery update query and returns the results.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <returns>A list of delivery updates that match the query.</returns>
    public async Task<List<DeliveryUpdate>> GetDeliveryUpdatesAsync(IQueryable<DeliveryUpdate> query)
    {
        return await query.ToListAsync();
    }

    /// <summary>
    /// Retrieves all delivery updates for a specific delivery.
    /// </summary>
    /// <param name="deliveryId">The ID of the delivery.</param>
    /// <returns>A list of matching delivery updates.</returns>
    public async Task<List<DeliveryUpdate>> GetDeliveryUpdatesByDeliveryAsync(int deliveryId)
    {
        return await _context.DeliveryUpdates
            .Where(du => du.DeliveryID == deliveryId)
            .Include(du => du.UpdatedByUser)
            .OrderByDescending(du => du.UpdateDate)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves delivery updates by delivery status.
    /// </summary>
    /// <param name="status">The delivery status to filter by.</param>
    /// <returns>A list of matching delivery updates.</returns>
    public async Task<List<DeliveryUpdate>> GetDeliveryUpdatesByStatusAsync(DeliveryStatus status)
    {
        return await _context.DeliveryUpdates
            .Where(du => du.Status == status)
            .Include(du => du.Delivery)
            .Include(du => du.UpdatedByUser)
            .OrderByDescending(du => du.UpdateDate)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves the most recent delivery updates up to the specified count.
    /// </summary>
    /// <param name="count">The maximum number of delivery updates to return.</param>
    /// <returns>A list of recent delivery updates.</returns>
    public async Task<List<DeliveryUpdate>> GetRecentDeliveryUpdatesAsync(int count = 10)
    {
        return await _context.DeliveryUpdates
            .Include(du => du.Delivery)
            .Include(du => du.UpdatedByUser)
            .OrderByDescending(du => du.UpdateDate)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// Updates an existing delivery update record.
    /// </summary>
    /// <param name="deliveryUpdate">The delivery update to update.</param>
    public async Task UpdateDeliveryUpdateAsync(DeliveryUpdate deliveryUpdate)
    {
        _context.DeliveryUpdates.Update(deliveryUpdate);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a delivery update record by its ID.
    /// </summary>
    /// <param name="deliveryUpdateId">The ID of the delivery update to delete.</param>
    public async Task DeleteDeliveryUpdateAsync(int deliveryUpdateId)
    {
        var deliveryUpdate = await _context.DeliveryUpdates.FindAsync(deliveryUpdateId);
        if (deliveryUpdate != null)
        {
            _context.DeliveryUpdates.Remove(deliveryUpdate);
            await _context.SaveChangesAsync();
        }
    }
}