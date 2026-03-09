using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Entities.Financial;
using PESYONG.Domain.Enums;
using PESYONG.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.Repositories;

/// <summary>
/// Provides data access operations for payments, including creation,
/// retrieval, status updates, modification, and deletion.
/// </summary>
public class PaymentRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentRepository"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new payment record in the database.
    /// </summary>
    /// <param name="payment">The payment to create.</param>
    public async Task CreatePaymentAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves a payment by its ID.
    /// </summary>
    /// <param name="id">The ID of the payment.</param>
    /// <returns>The matching payment if found; otherwise, <c>null</c>.</returns>
    public async Task<Payment?> GetPaymentByIdAsync(string id)
    {
        return await _context.Payments.FindAsync(id);
    }

    /// <summary>
    /// Retrieves all payments ordered by most recent timestamp.
    /// </summary>
    /// <returns>A list of all payments.</returns>
    public async Task<List<Payment>> GetAllPaymentsAsync()
    {
        return await _context.Payments
            .OrderByDescending(p => p.TimeStamp)
            .ToListAsync();
    }

    /// <summary>
    /// Updates the status of a payment.
    /// </summary>
    /// <param name="paymentId">The ID of the payment to update.</param>
    /// <param name="newStatus">The new payment status.</param>
    public async Task UpdatePaymentStatusAsync(string paymentId, PaymentStatus newStatus)
    {
        var payment = await _context.Payments.FindAsync(paymentId);
        if (payment != null)
        {
            payment.PaymentStatus = newStatus;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Updates an existing payment record.
    /// </summary>
    /// <param name="payment">The payment to update.</param>
    public async Task UpdatePaymentAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a payment record by its ID.
    /// </summary>
    /// <param name="paymentId">The ID of the payment to delete.</param>
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