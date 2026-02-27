using PESYONG.Domain.Entities.Orders;
using PESYONG.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.Repositories;
public class OrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task SaveOrderAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
    }

    public List<Order> GetOrderHistory(int userId)
    {
        return _context.Orders
            .Where(o => o.RecipientID== userId)
            .OrderByDescending(o => o.OrderDate)
            .ToList();
    }
}
