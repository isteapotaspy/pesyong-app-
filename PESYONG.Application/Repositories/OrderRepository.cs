using Microsoft.EntityFrameworkCore;
using PESYONG.ApplicationLogic.DTOs;
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

public class OrderRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public OrderRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Guid> PlaceOrderAsync(CheckoutRequestDto request)
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
            SpecialInstructions = request.Notes,
            OrderDate = DateTime.Now,
            EstimatedDeliveryDate = request.EstimatedDeliveryDate,
            DeliveryType = DeliveryStatus.Pending,
            DeliveryStatus = DeliveryStatus.Pending
        };

        foreach (var item in request.Items)
        {
            MealProduct mealProduct;

            if (item.Type == "catering" &&
                item.CateringSelections != null &&
                item.CateringSelections.Count > 0)
            {
                var mealProductItems = new List<MealProductItem>();

                foreach (var selection in item.CateringSelections)
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

                mealProduct = new MealProduct
                {
                    OwnerID = null,
                    ProductName = $"Custom Package - {DateTime.Now:MM/dd/yyyy}",
                    IsCateringPackage = true,
                    MealProductItems = mealProductItems
                };

                context.MealProducts.Add(mealProduct);
                await context.SaveChangesAsync();
            }
            else if (item.Type == "shortorder")
            {
                var meal = await context.Meals.FindAsync(item.ProductID);
                if (meal == null || !meal.MealID.HasValue)
                {
                    throw new InvalidOperationException(
                        $"Meal with ID {item.ProductID} was not found.");
                }

                mealProduct = new MealProduct
                {
                    OwnerID = null,
                    ProductName = meal.MealName,
                    IsCateringPackage = false,
                    MealProductItems = new List<MealProductItem>
                {
                    new MealProductItem
                    {
                        MealID = meal.MealID.Value,
                        Meal = meal,
                        Quantity = 1
                    }
                }
                };

                context.MealProducts.Add(mealProduct);
                await context.SaveChangesAsync();
            }
            else if (item.Type == "package")
            {
                var existingMealProduct = await context.MealProducts.FindAsync(item.ProductID);
                if (existingMealProduct == null)
                {
                    throw new InvalidOperationException(
                        $"Package with MealProductID {item.ProductID} was not found.");
                }

                mealProduct = existingMealProduct;
            }
            else if (item.Type == "kakanin")
            {
                // This depends on how kakanin is stored in your DB.
                // If kakanin is also stored in Meals, use Meals.FindAsync(item.ProductId).
                // If not, you need a separate branch for its actual table.
                var meal = await context.Meals.FindAsync(item.ProductID);
                if (meal == null || !meal.MealID.HasValue)
                {
                    throw new InvalidOperationException(
                        $"Kakanin item with ID {item.ProductID} was not found.");
                }

                mealProduct = new MealProduct
                {
                    OwnerID = null,
                    ProductName = meal.MealName,
                    IsCateringPackage = false,
                    MealProductItems = new List<MealProductItem>
                {
                    new MealProductItem
                    {
                        MealID = meal.MealID.Value,
                        Meal = meal,
                        Quantity = 1
                    }
                }
                };

                context.MealProducts.Add(mealProduct);
                await context.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException(
                    $"Unsupported cart item type: {item.Type}");
            }

            order.OrderItems.Add(new OrderMealProduct
            {
                OrderID = order.OrderID,
                MealProductID = mealProduct.MealProductID,
                MealProductOrderQty = item.Quantity,
                ItemPrice = mealProduct.FinalPrice
            });
        }

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        return order.OrderID;
    }

    public async Task<Order?> GetOrderByIdAsync(Guid id)
    {
        using var context = _contextFactory.CreateDbContext();

        return await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Receipt)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderID == id);
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        using var context = _contextFactory.CreateDbContext();

        return await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersByCustomerAsync(Guid customerId)
    {
        using var context = _contextFactory.CreateDbContext();

        return await context.Orders
            .Where(o => o.CustomerID == customerId)
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersByStatusAsync(DeliveryStatus status)
    {
        using var context = _contextFactory.CreateDbContext();

        return await context.Orders
            .Where(o => o.DeliveryStatus == status)
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetCartOrdersAsync()
    {
        using var context = _contextFactory.CreateDbContext();

        return await context.Orders
            .Where(o => o.DeliveryType == DeliveryStatus.OnCart)
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task UpdateOrderAsync(Order order)
    {
        using var context = _contextFactory.CreateDbContext();

        context.Orders.Update(order);
        await context.SaveChangesAsync();
    }

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
}