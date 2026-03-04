using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Enums;
using PESYONG.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.Repositories;

public class AcknowledgementReceiptRepository
{
    private readonly AppDbContext _context;

    public AcknowledgementReceiptRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateAcknowledgementReceiptAsync(AcknowledgementReceipt receipt)
    {
        _context.AcknowledgementReceipts.Add(receipt);
        await _context.SaveChangesAsync();
    }

    public async Task<AcknowledgementReceipt> GetAcknowledgementReceiptByIdAsync(int id)
    {
        return await _context.AcknowledgementReceipts.FindAsync(id);
    }

    public async Task<List<AcknowledgementReceipt>> GetAllAcknowledgementReceiptsAsync()
    {
        return await _context.AcknowledgementReceipts.OrderByDescending(ar => ar.IssueDate).ToListAsync();
    }

    public async Task UpdateAcknowledgementReceiptAsync(AcknowledgementReceipt receipt)
    {
        _context.AcknowledgementReceipts.Update(receipt);
        await _context.SaveChangesAsync();
    }

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
