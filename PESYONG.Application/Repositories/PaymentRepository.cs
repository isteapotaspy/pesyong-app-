using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Entities.Financial;
using PESYONG.Domain.Enums;
using PESYONG.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.Repositories;

public class PaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreatePaymentAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
    }

    public async Task<Payment> GetPaymentByIdAsync(string id)
    {
        return await _context.Payments.FindAsync(id);
    }

    public async Task<List<Payment>> GetAllPaymentsAsync()
    {
        return await _context.Payments.OrderByDescending(p => p.TimeStamp).ToListAsync();
    }

    public async Task UpdatePaymentStatusAsync(string paymentId, PaymentStatus newStatus)
    {
        var payment = await _context.Payments.FindAsync(paymentId);
        if (payment != null)
        {
            payment.PaymentStatus = newStatus;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdatePaymentAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePaymentAsync(string paymentId)
    {
        var payment = await _context.Payments.FindAsync(paymentId);
        if (payment != null)
        {
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
        }
    }
}
