using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Entities.Logistics;
using PESYONG.Domain.Enums;
using PESYONG.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.Repositories;

public class DeliveryUpdateRepository
{
    private readonly AppDbContext _context;

    public DeliveryUpdateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateDeliveryUpdateAsync(DeliveryUpdate deliveryUpdate)
    {
        _context.DeliveryUpdates.Add(deliveryUpdate);
        await _context.SaveChangesAsync();
    }

    public async Task<DeliveryUpdate> GetDeliveryUpdateByIdAsync(int id)
    {
        return await _context.DeliveryUpdates
            .Include(du => du.Delivery)
            .Include(du => du.UpdatedByUser)
            .FirstOrDefaultAsync(du => du.DeliveryUpdateID == id);
    }

    public async Task<List<DeliveryUpdate>> GetAllDeliveryUpdatesAsync()
    {
        return await _context.DeliveryUpdates
            .Include(du => du.Delivery)
            .Include(du => du.UpdatedByUser)
            .OrderByDescending(du => du.UpdateDate)
            .ToListAsync();
    }

    public async Task<List<DeliveryUpdate>> GetDeliveryUpdatesAsync(IQueryable<DeliveryUpdate> query)
    {
        return await query.ToListAsync();
    }

    public async Task<List<DeliveryUpdate>> GetDeliveryUpdatesByDeliveryAsync(int deliveryId)
    {
        return await _context.DeliveryUpdates
            .Where(du => du.DeliveryID == deliveryId)
            .Include(du => du.UpdatedByUser)
            .OrderByDescending(du => du.UpdateDate)
            .ToListAsync();
    }

    public async Task<List<DeliveryUpdate>> GetDeliveryUpdatesByStatusAsync(DeliveryStatus status)
    {
        return await _context.DeliveryUpdates
            .Where(du => du.Status == status)
            .Include(du => du.Delivery)
            .Include(du => du.UpdatedByUser)
            .OrderByDescending(du => du.UpdateDate)
            .ToListAsync();
    }

    public async Task<List<DeliveryUpdate>> GetRecentDeliveryUpdatesAsync(int count = 10)
    {
        return await _context.DeliveryUpdates
            .Include(du => du.Delivery)
            .Include(du => du.UpdatedByUser)
            .OrderByDescending(du => du.UpdateDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task UpdateDeliveryUpdateAsync(DeliveryUpdate deliveryUpdate)
    {
        _context.DeliveryUpdates.Update(deliveryUpdate);
        await _context.SaveChangesAsync();
    }

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
