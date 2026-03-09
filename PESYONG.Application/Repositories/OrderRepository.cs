using Microsoft.EntityFrameworkCore;
using PESYONG.ApplicationLogic.DTOs;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Meals.MealProduct;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Entities.Users;
using PESYONG.Domain.Enums;
using PESYONG.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.Repositories;

/// <summary>
/// Provides data access operations for orders, including order placement,
/// retrieval, filtering, updates, receipt assignment, and deletion.
/// </summary>
public class OrderRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderRepository"/> class.
    /// </summary>
    /// <param name="contextFactory">The database context factory used to create application database contexts.</param>
    public OrderRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Places a new order using the provided checkout request data.
    /// </summary>
    /// <param name="request">The checkout request containing customer and item details.</param>
    /// <returns>The ID of the newly created order.</returns>
    public async Task<OrderPlacementResultDto> PlaceOrderAsync(CheckoutRequestDto request)
    {
        using var context = _contextFactory.CreateDbContext();

        var customer = await context.Customers
            .FirstOrDefaultAsync(c =>
                c.PhoneNumber == request.PhoneNumber ||
                (!string.IsNullOrWhiteSpace(request.Email) && c.Email == request.Email));

        if (customer == null)
        {
            customer = new Customer
            {
                CustomerID = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email,
                Address = request.Address,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            context.Customers.Add(customer);
        }
        else
        {
            customer.FirstName = request.FirstName;
            customer.LastName = request.LastName;
            customer.PhoneNumber = request.PhoneNumber;
            customer.Email = string.IsNullOrWhiteSpace(request.Email) ? customer.Email : request.Email;
            customer.Address = request.Address;
        }

        var order = new Order
        {
            OrderID = Guid.NewGuid(),
            CustomerID = customer.CustomerID,
            Address = request.Address,
            CustomerNotes = request.Notes,
            SpecialInstructions = null,
            OrderDate = DateTime.Now,
            EstimatedDeliveryDate = request.EstimatedDeliveryDate,
            DeliveryType = request.DeliveryType,
            DeliveryStatus = DeliveryStatus.Pending
        };

        var groupedNormalItems = request.Items
            .Where(x => !(x.Type == "package" &&
                          x.CateringSelections != null &&
                          x.CateringSelections.Count > 0))
            .GroupBy(x => new { x.ProductID, x.Type, x.ItemPrice })
            .ToList();

        var customPackageItems = request.Items
            .Where(x => x.Type == "package" &&
                        x.CateringSelections != null &&
                        x.CateringSelections.Count > 0)
            .ToList();

        foreach (var group in groupedNormalItems)
        {
            var firstItem = group.First();

            var mealProduct = await context.MealProducts.FindAsync(firstItem.ProductID)
                ?? throw new InvalidOperationException(
                    $"MealProduct with ID {firstItem.ProductID} was not found.");

            order.OrderItems.Add(new OrderMealProduct
            {
                OrderID = order.OrderID,
                MealProductID = mealProduct.MealProductID,
                MealProductOrderQty = group.Sum(x => x.Quantity),
                ItemPrice = firstItem.ItemPrice
            });
        }

        foreach (var item in customPackageItems)
        {
            var mealProductItems = new List<MealProductItem>();

            foreach (var selection in item.CateringSelections!)
            {
                var meal = await context.Meals.FindAsync(selection.MealId);
                if (meal == null || !meal.MealID.HasValue)
                {
                    throw new InvalidOperationException(
                        $"Meal with ID {selection.MealId} was not found.");
                }

                mealProductItems.Add(new MealProductItem
                {
                    MealID = meal.MealID.Value,
                    Meal = meal,
                    Quantity = 1
                });
            }

            var customMealProduct = new MealProduct
            {
                OwnerID = null,
                ProductName = $"Custom Package - {DateTime.Now:MM/dd/yyyy}",
                IsCateringPackage = true,
                MealProductItems = mealProductItems
            };

            context.MealProducts.Add(customMealProduct);
            await context.SaveChangesAsync();

            order.OrderItems.Add(new OrderMealProduct
            {
                OrderID = order.OrderID,
                MealProductID = customMealProduct.MealProductID,
                MealProductOrderQty = item.Quantity,
                ItemPrice = item.ItemPrice
            });
        }

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        return new OrderPlacementResultDto
        {
            OrderId = order.OrderID,
            CustomerId = customer.CustomerID
        };
    }

    /// <summary>
    /// Retrieves an order by its ID.
    /// </summary>
    /// <param name="id">The ID of the order to retrieve.</param>
    /// <returns>The matching order if found; otherwise, <c>null</c>.</returns>
    public async Task<Order?> GetOrderByIdAsync(Guid id)
    {
        using var context = _contextFactory.CreateDbContext();

        return await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Receipt)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MealProduct)
            .FirstOrDefaultAsync(o => o.OrderID == id);
    }

    /// <summary>
    /// Retrieves all orders ordered by most recent order date.
    /// </summary>
    /// <returns>A list of all orders.</returns>
    public async Task<List<Order>> GetAllOrdersAsync()
    {
        using var context = _contextFactory.CreateDbContext();

        return await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MealProduct)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all orders for a specific customer.
    /// </summary>
    /// <param name="customerId">The ID of the customer.</param>
    /// <returns>A list of orders belonging to the customer.</returns>
    public async Task<List<Order>> GetOrdersByCustomerAsync(Guid customerId)
    {
        using var context = _contextFactory.CreateDbContext();

        return await context.Orders
            .Where(o => o.CustomerID == customerId)
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MealProduct)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all orders with a specific delivery status.
    /// </summary>
    /// <param name="status">The delivery status to filter by.</param>
    /// <returns>A list of matching orders.</returns>
    public async Task<List<Order>> GetOrdersByStatusAsync(DeliveryStatus status)
    {
        using var context = _contextFactory.CreateDbContext();

        return await context.Orders
            .Where(o => o.DeliveryStatus == status)
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MealProduct)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all orders currently marked as on-cart.
    /// </summary>
    /// <returns>A list of cart orders.</returns>
    public async Task<List<Order>> GetCartOrdersAsync()
    {
        using var context = _contextFactory.CreateDbContext();

        return await context.Orders
            .Where(o => o.DeliveryStatus == DeliveryStatus.OnCart)
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MealProduct)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    /// <summary>
    /// Updates an existing order.
    /// </summary>
    /// <param name="order">The order object containing updated values.</param>
    public async Task UpdateOrderAsync(Order order)
    {
        using var context = _contextFactory.CreateDbContext();

        context.Orders.Update(order);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates the delivery status of an order.
    /// </summary>
    /// <param name="orderId">The ID of the order to update.</param>
    /// <param name="newStatus">The new delivery status.</param>
    public async Task UpdateOrderStatusAsync(Guid orderId, DeliveryStatus newStatus)
    {
        using var context = _contextFactory.CreateDbContext();

        var order = await context.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.DeliveryStatus = newStatus;
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Assigns a receipt to an order.
    /// </summary>
    /// <param name="orderId">The ID of the order to update.</param>
    /// <param name="receiptId">The ID of the receipt to assign.</param>
    public async Task AssignReceiptAsync(Guid orderId, int receiptId)
    {
        using var context = _contextFactory.CreateDbContext();

        var order = await context.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.ReceiptID = receiptId;
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Deletes an order by its ID.
    /// </summary>
    /// <param name="orderId">The ID of the order to delete.</param>
    public async Task DeleteOrderAsync(Guid orderId)
    {
        using var context = _contextFactory.CreateDbContext();

        var order = await context.Orders.FindAsync(orderId);
        if (order != null)
        {
            context.Orders.Remove(order);
            await context.SaveChangesAsync();
        }
    }

    public async Task<Order> CreateOrderAsyncReturnSelf(Order order)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        return order ;
    }
}