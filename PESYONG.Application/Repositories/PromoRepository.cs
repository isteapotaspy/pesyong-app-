using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Entities.Financial.Promos;
using PESYONG.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.Repositories;

public class PromoRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public PromoRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task CreatePromoAsync(Promo promo)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.Set<Promo>().Add(promo);
        Debug.WriteLine($"Creating promo: {promo.Code}");
        await context.SaveChangesAsync();
    }

    public async Task<Promo> CreatePromoAsyncReturnSelf(Promo promo)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.Set<Promo>().Add(promo);
        await context.SaveChangesAsync();

        Debug.WriteLine($"Created promo ID {promo.PromoID}: {promo.Code}");
        return promo;
    }

    public async Task<Promo?> GetPromoByIdAsync(int promoId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Set<Promo>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PromoID == promoId);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetPromoByIdAsync error: {ex}");
            return null;
        }
    }

    public async Task<Promo?> GetPromoByCodeAsync(string code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            await using var context = await _contextFactory.CreateDbContextAsync();

            var normalizedCode = code.Trim().ToUpperInvariant();

            return await context.Set<Promo>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == normalizedCode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetPromoByCodeAsync error: {ex}");
            return null;
        }
    }

    public async Task<List<Promo>> GetAllPromosAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var promos = await context.Set<Promo>()
                .AsNoTracking()
                .OrderByDescending(x => x.ValidFrom)
                .ToListAsync();

            Debug.WriteLine($"Retrieved {promos.Count} promos.");
            return promos;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetAllPromosAsync error: {ex}");
            return new List<Promo>();
        }
    }

    public async Task<List<Promo>> GetPromosAsync(Func<IQueryable<Promo>, IQueryable<Promo>> queryBuilder)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        IQueryable<Promo> query = context.Set<Promo>();
        query = queryBuilder(query);

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<bool> ValidatePromoAsync(string code, decimal orderAmount)
    {
        try
        {
            var promo = await GetPromoByCodeAsync(code);
            if (promo == null)
                return false;

            return promo.IsActive
                && (!promo.MinimumOrderAmount.HasValue || orderAmount >= promo.MinimumOrderAmount.Value);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ValidatePromoAsync error: {ex}");
            return false;
        }
    }

    public async Task UpdatePromoAsync(Promo promo)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.Set<Promo>().Update(promo);
        await context.SaveChangesAsync();

        Debug.WriteLine($"Updated promo ID {promo.PromoID}");
    }

    public async Task IncrementUsageAsync(int promoId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var promo = await context.Set<Promo>().FirstOrDefaultAsync(x => x.PromoID == promoId);
            if (promo == null)
            {
                Debug.WriteLine($"IncrementUsageAsync skipped. Promo ID {promoId} not found.");
                return;
            }

            promo.UsedCount++;
            await context.SaveChangesAsync();

            Debug.WriteLine($"Incremented usage for promo ID {promoId}. UsedCount={promo.UsedCount}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"IncrementUsageAsync error: {ex}");
            throw;
        }
    }

    public async Task DeletePromoAsync(int promoId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var entity = await context.Set<Promo>().FirstOrDefaultAsync(x => x.PromoID == promoId);
        if (entity == null)
        {
            Debug.WriteLine($"Delete skipped. Promo ID {promoId} not found.");
            return;
        }

        context.Set<Promo>().Remove(entity);
        await context.SaveChangesAsync();

        Debug.WriteLine($"Deleted promo ID {promoId}");
    }
}