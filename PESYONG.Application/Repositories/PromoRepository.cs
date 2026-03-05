using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Entities.Financial.Promos;
using PESYONG.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.Repositories;

public class PromoRepository
{
    private readonly AppDbContext _context;

    public PromoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreatePromoAsync(Promo promo)
    {
        _context.Promos.Add(promo);
        await _context.SaveChangesAsync();
    }

    public async Task<Promo> GetPromoByIdAsync(int id)
    {
        return await _context.Promos.FindAsync(id);
    }

    public async Task<Promo> GetPromoByCodeAsync(string code)
    {
        return await _context.Promos.FirstOrDefaultAsync(p => p.Code == code);
    }

    public async Task<List<Promo>> GetAllPromosAsync()
    {
        return await _context.Promos.OrderByDescending(p => p.ValidFrom).ToListAsync();
    }

    public async Task<bool> ValidatePromoAsync(string code, decimal orderAmount)
    {
        var promo = await GetPromoByCodeAsync(code);
        if (promo == null) return false;
        return promo.IsActive && (!promo.MinimumOrderAmount.HasValue || orderAmount >= promo.MinimumOrderAmount.Value);
    }

    public async Task IncrementUsageAsync(int promoId)
    {
        var promo = await _context.Promos.FindAsync(promoId);
        if (promo != null)
        {
            promo.UsedCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdatePromoAsync(Promo promo)
    {
        _context.Promos.Update(promo);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePromoAsync(int promoId)
    {
        var promo = await _context.Promos.FindAsync(promoId);
        if (promo != null)
        {
            _context.Promos.Remove(promo);
            await _context.SaveChangesAsync();
        }
    }
}
