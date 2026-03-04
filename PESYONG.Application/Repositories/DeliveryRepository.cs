using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Entities.Logistics;
using PESYONG.Domain.Enums;
using PESYONG.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.Repositories;

public class DeliveryRepository
{
    private readonly AppDbContext _context;

    public DeliveryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateDeliveryAsync(Delivery delivery)
    {
        _context.Deliveries.Add(delivery);
        await _context.SaveChangesAsync();
    }

    public async Task<Delivery> GetDeliveryByIdAsync(int id)
    {
        return await _context.Deliveries
            .Include(d => d.Order)
            .Include(d => d.DeliveryPersonnel)
            .Include(d => d.DeliveryUpdates)
            .FirstOrDefaultAsync(d => d.DeliveryID == id);
    }

    public async Task<List<Delivery>> GetAllDeliveriesAsync()
    {
        return await _context.Deliveries
            .Include(d => d.Order)
            .Include(d => d.DeliveryPersonnel)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<Delivery>> GetDeliveriesAsync(IQueryable<Delivery> query)
    {
        return await query.ToListAsync();
    }

    public async Task<List<Delivery>> GetDeliveriesByStatusAsync(DeliveryStatus status)
    {
        return await _context.Deliveries
            .Where(d => d.Status == status)
            .Include(d => d.Order)
            .Include(d => d.DeliveryPersonnel)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<Delivery>> GetDeliveriesByPersonnelAsync(int personnelId)
    {
        return await _context.Deliveries
            .Where(d => d.DeliveryPersonnelID == personnelId)
            .Include(d => d.Order)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<Delivery>> GetPendingDeliveriesAsync()
    {
        return await _context.Deliveries
            .Where(d => d.Status == DeliveryStatus.Pending)
            .Include(d => d.Order)
            .Include(d => d.DeliveryPersonnel)
            .OrderBy(d => d.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<Delivery>> GetInTransitDeliveriesAsync()
    {
        return await _context.Deliveries
            .Where(d => d.Status == DeliveryStatus.InTransit)
            .Include(d => d.Order)
            .Include(d => d.DeliveryPersonnel)
            .OrderBy(d => d.CreatedDate)
            .ToListAsync();
    }

    public async Task UpdateDeliveryAsync(Delivery delivery)
    {
        _context.Deliveries.Update(delivery);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateDeliveryStatusAsync(int deliveryId, DeliveryStatus newStatus)
    {
        var delivery = await _context.Deliveries.FindAsync(deliveryId);
        if (delivery != null)
        {
            delivery.Status = newStatus;
            await _context.SaveChangesAsync();
        }
    }

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

    public async Task AssignDeliveryPersonnelAsync(int deliveryId, int personnelId)
    {
        var delivery = await _context.Deliveries.FindAsync(deliveryId);
        if (delivery != null)
        {
            delivery.DeliveryPersonnelID = personnelId;
            await _context.SaveChangesAsync();
        }
    }

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
