using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Enums;
using PESYONG.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.Repositories;

public class OrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateOrderAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
    }

    public async Task<Order> GetOrderByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.Recipient)
            .Include(o => o.Receipt)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderID == id);
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await _context.Orders
            .Include(o => o.Recipient)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersByRecipientAsync(int recipientId)
    {
        return await _context.Orders
            .Where(o => o.RecipientID == recipientId)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersByStatusAsync(DeliveryStatus status)
    {
        return await _context.Orders
            .Where(o => o.DeliveryStatus == status)
            .Include(o => o.Recipient)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetCartOrdersAsync()
    {
        return await _context.Orders
            .Where(o => o.DeliveryType == DeliveryStatus.OnCart)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task UpdateOrderAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateOrderStatusAsync(Guid orderId, DeliveryStatus newStatus)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.DeliveryStatus = newStatus;
            await _context.SaveChangesAsync();
        }
    }

    public async Task AssignReceiptAsync(Guid orderId, int receiptId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.ReceiptID = receiptId;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteOrderAsync(Guid orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }
}

public class OrderMealProductRepository
{
    private readonly AppDbContext _context;

    public OrderMealProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddOrderItemAsync(OrderMealProduct orderItem)
    {
        _context.OrderMealProducts.Add(orderItem);
        await _context.SaveChangesAsync();
    }

    public async Task<List<OrderMealProduct>> GetOrderItemsByOrderAsync(Guid orderId)
    {
        return await _context.OrderMealProducts
            .Where(oi => oi.OrderID == orderId)
            .ToListAsync();
    }

    public async Task UpdateOrderItemQuantityAsync(Guid orderId, int mealProductId, int newQuantity)
    {
        var orderItem = await _context.OrderMealProducts
            .FirstOrDefaultAsync(oi => oi.OrderID == orderId && oi.MealProductID == mealProductId);

        if (orderItem != null)
        {
            orderItem.MealProductOrderQty = newQuantity;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveOrderItemAsync(Guid orderId, int mealProductId)
    {
        var orderItem = await _context.OrderMealProducts
            .FirstOrDefaultAsync(oi => oi.OrderID == orderId && oi.MealProductID == mealProductId);

        if (orderItem != null)
        {
            _context.OrderMealProducts.Remove(orderItem);
            await _context.SaveChangesAsync();
        }
    }
}
