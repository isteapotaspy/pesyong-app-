using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Enums;
using PESYONG.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.Repositories;

/// <summary>
/// Provides data access operations for acknowledgement receipts.
/// </summary>
public class AcknowledgementReceiptRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AcknowledgementReceiptRepository"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public AcknowledgementReceiptRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new acknowledgement receipt in the database.
    /// </summary>
    /// <param name="receipt">The acknowledgement receipt to create.</param>
    public async Task CreateAcknowledgementReceiptAsync(AcknowledgementReceipt receipt)
    {
        _context.AcknowledgementReceipts.Add(receipt);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves an acknowledgement receipt by its ID.
    /// </summary>
    /// <param name="id">The ID of the acknowledgement receipt.</param>
    /// <returns>The matching acknowledgement receipt, if found.</returns>
    public async Task<AcknowledgementReceipt> GetAcknowledgementReceiptByIdAsync(int id)
    {
        return await _context.AcknowledgementReceipts.FindAsync(id);
    }

    /// <summary>
    /// Retrieves all acknowledgement receipts ordered by most recent issue date.
    /// </summary>
    /// <returns>A list of all acknowledgement receipts.</returns>
    public async Task<List<AcknowledgementReceipt>> GetAllAcknowledgementReceiptsAsync()
    {
        return await _context.AcknowledgementReceipts
            .OrderByDescending(ar => ar.IssueDate)
            .ToListAsync();
    }

    /// <summary>
    /// Updates an existing acknowledgement receipt.
    /// </summary>
    /// <param name="receipt">The acknowledgement receipt to update.</param>
    public async Task UpdateAcknowledgementReceiptAsync(AcknowledgementReceipt receipt)
    {
        _context.AcknowledgementReceipts.Update(receipt);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Marks an acknowledgement receipt as paid and sets its payment date.
    /// </summary>
    /// <param name="receiptId">The ID of the receipt to update.</param>
    /// <param name="paymentDate">The payment date to record.</param>
    public async Task MarkAsPaidAsync(int receiptId, DateTime paymentDate)
    {
        var receipt = await _context.AcknowledgementReceipts.FindAsync(receiptId);
        if (receipt != null)
        {
            receipt.PaymentDate = paymentDate;
            receipt.Status = PaymentStatus.Completed;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Deletes an acknowledgement receipt from the database.
    /// </summary>
    /// <param name="receiptId">The ID of the receipt to delete.</param>
    public async Task DeleteAcknowledgementReceiptAsync(int receiptId)
    {
        var receipt = await _context.AcknowledgementReceipts.FindAsync(receiptId);
        if (receipt != null)
        {
            _context.AcknowledgementReceipts.Remove(receipt);
            await _context.SaveChangesAsync();
        }
    }
}