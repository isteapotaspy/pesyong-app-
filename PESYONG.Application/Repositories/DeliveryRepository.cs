using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Entities.Logistics;
using PESYONG.Domain.Enums;
using PESYONG.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.Repositories;

/// <summary>
/// Provides data access operations for deliveries, including creation,
/// retrieval, assignment, status updates, and deletion.
/// </summary>
public class DeliveryRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeliveryRepository"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public DeliveryRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new delivery record in the database.
    /// </summary>
    /// <param name="delivery">The delivery to create.</param>
    public async Task CreateDeliveryAsync(Delivery delivery)
    {
        _context.Deliveries.Add(delivery);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves a delivery by its ID, including related order,
    /// delivery personnel, and delivery updates.
    /// </summary>
    /// <param name="id">The ID of the delivery.</param>
    /// <returns>The matching delivery, if found.</returns>
    public async Task<Delivery?> GetDeliveryByIdAsync(int id)
    {
        return await _context.Deliveries
            .Include(d => d.Order)
            .Include(d => d.DeliveryPersonnel)
            .Include(d => d.DeliveryUpdates)
            .FirstOrDefaultAsync(d => d.DeliveryID == id);
    }

    /// <summary>
    /// Retrieves all deliveries ordered by most recent creation date.
    /// </summary>
    /// <returns>A list of all deliveries.</returns>
    public async Task<List<Delivery>> GetAllDeliveriesAsync()
    {
        return await _context.Deliveries
            .Include(d => d.Order)
            .Include(d => d.DeliveryPersonnel)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    /// <summary>
    /// Executes a custom delivery query and returns the results.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <returns>A list of deliveries that match the query.</returns>
    public async Task<List<Delivery>> GetDeliveriesAsync(IQueryable<Delivery> query)
    {
        return await query.ToListAsync();
    }

    /// <summary>
    /// Retrieves deliveries by their status.
    /// </summary>
    /// <param name="status">The delivery status to filter by.</param>
    /// <returns>A list of matching deliveries.</returns>
    public async Task<List<Delivery>> GetDeliveriesByStatusAsync(DeliveryStatus status)
    {
        return await _context.Deliveries
            .Where(d => d.Status == status)
            .Include(d => d.Order)
            .Include(d => d.DeliveryPersonnel)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves deliveries assigned to a specific delivery personnel.
    /// </summary>
    /// <param name="personnelId">The ID of the delivery personnel.</param>
    /// <returns>A list of matching deliveries.</returns>
    public async Task<List<Delivery>> GetDeliveriesByPersonnelAsync(int personnelId)
    {
        return await _context.Deliveries
            .Where(d => d.DeliveryPersonnelID == personnelId)
            .Include(d => d.Order)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all pending deliveries.
    /// </summary>
    /// <returns>A list of pending deliveries.</returns>
    public async Task<List<Delivery>> GetPendingDeliveriesAsync()
    {
        return await _context.Deliveries
            .Where(d => d.Status == DeliveryStatus.Pending)
            .Include(d => d.Order)
            .Include(d => d.DeliveryPersonnel)
            .OrderBy(d => d.CreatedDate)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all deliveries that are currently in transit.
    /// </summary>
    /// <returns>A list of in-transit deliveries.</returns>
    public async Task<List<Delivery>> GetInTransitDeliveriesAsync()
    {
        return await _context.Deliveries
            .Where(d => d.Status == DeliveryStatus.InTransit)
            .Include(d => d.Order)
            .Include(d => d.DeliveryPersonnel)
            .OrderBy(d => d.CreatedDate)
            .ToListAsync();
    }

    /// <summary>
    /// Updates an existing delivery record.
    /// </summary>
    /// <param name="delivery">The delivery to update.</param>
    public async Task UpdateDeliveryAsync(Delivery delivery)
    {
        _context.Deliveries.Update(delivery);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates the status of a delivery.
    /// </summary>
    /// <param name="deliveryId">The ID of the delivery to update.</param>
    /// <param name="newStatus">The new delivery status.</param>
    public async Task UpdateDeliveryStatusAsync(int deliveryId, DeliveryStatus newStatus)
    {
        var delivery = await _context.Deliveries.FindAsync(deliveryId);
        if (delivery != null)
        {
            delivery.Status = newStatus;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Marks a delivery as delivered and records receipt details.
    /// </summary>
    /// <param name="deliveryId">The ID of the delivery to update.</param>
    /// <param name="deliveryDate">The actual delivery date and time.</param>
    /// <param name="receivedBy">The name of the person who received the delivery.</param>
    public async Task MarkAsDeliveredAsync(int deliveryId, DateTime deliveryDate, string receivedBy)
    {
        var delivery = await _context.Deliveries.FindAsync(deliveryId);
        if (delivery != null)
        {
            delivery.Status = DeliveryStatus.Delivered;
            delivery.ActualDeliveryDate = deliveryDate;
            delivery.ReceivedBy = receivedBy;
            delivery.ReceivedAt = deliveryDate;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Assigns delivery personnel to a delivery.
    /// </summary>
    /// <param name="deliveryId">The ID of the delivery.</param>
    /// <param name="personnelId">The ID of the delivery personnel to assign.</param>
    public async Task AssignDeliveryPersonnelAsync(int deliveryId, int personnelId)
    {
        var delivery = await _context.Deliveries.FindAsync(deliveryId);
        if (delivery != null)
        {
            delivery.DeliveryPersonnelID = personnelId;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Deletes a delivery record by its ID.
    /// </summary>
    /// <param name="deliveryId">The ID of the delivery to delete.</param>
    public async Task DeleteDeliveryAsync(int deliveryId)
    {
        var delivery = await _context.Deliveries.FindAsync(deliveryId);
        if (delivery != null)
        {
            _context.Deliveries.Remove(delivery);
            await _context.SaveChangesAsync();
        }
    }
}