using PESYONG.Domain.Entities.Orders;
using PESYONG.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/// <summary>
/// This serves as the quasi-controller from an frontend-based API.
/// An ASP.NET backend wasn't implemented. Instead, the data doesn't 
/// go through an API layer and goes directly to the database. 
/// 
/// Speficially, chis handles the orders in the system. 
/// </summary>
 
/// <remarks>
/// TASK. Need to implement AuthorizationService, which will check data access patterns.
/// TASK. Need to implement LoggingService, which will log all transactions and bind it to an AppUser.
/// </remarks>
 
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
