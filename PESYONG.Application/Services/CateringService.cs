using AutoMapper;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Meals.MealProduct;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Enums;
using PESYONG.ApplicationLogic.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.Services;

public class CateringService
{
    private readonly IMapper _mapper;

    public CateringService(IMapper mapper) => _mapper = mapper;

    public List<MealSelectionDto> GetAvailableViands()
    {
        // This is mock data using your Meal entity structure
        var meals = new List<Meal>
        {
            new() { MealID = 1, MealName = "Lechon Kawali", MealPrice = 150.00m },
            new() { MealID = 2, MealName = "Pork Adobo", MealPrice = 120.00m },
            new() { MealID = 3, MealName = "Beef Caldereta", MealPrice = 180.00m }
        };

        return _mapper.Map<List<MealSelectionDto>>(meals);
    }

    public Order CreateOrderFromSelection(List<MealSelectionDto> selectedDtos, int customerId)
    {
        // Validation Logic
        if (selectedDtos.Count > 8)
            throw new InvalidOperationException("A custom package cannot exceed 8 viands.");

        // Map DTOs to MealProductItems 
        var productItems = _mapper.Map<List<MealProductItem>>(selectedDtos);

        //Create the MealProduct
        // Note: ProductBasePrice is [NotMapped] and calculated automatically 
        // by the Entity via Sum(item => item.ItemPrice). 
        var mealProduct = new MealProduct
        {
            OwnerID = customerId,
            ProductName = $"Custom Package - {DateTime.Now:MM/dd/yyyy}",
            MealProductItems = productItems
        };

        // Create the Order
        // Note: OrderDate is read-only and defaults to DateTime.Now in Order.
        var order = new Order
        {
            RecipientID = customerId,
            DeliveryType = DeliveryStatus.OnCart,   // "stored in user's cart IF NOT checked out"
            DeliveryStatus = DeliveryStatus.Pending,
            OrderID = Guid.NewGuid() // Explicitly setting the Key since it's a Guid
        };

        // Wrap the MealProduct into an OrderItem
        // Order uses OrderMealProduct (a join entity)
        order.OrderItems.Add(new OrderMealProduct
        {
            MealProduct = mealProduct,
            MealProductOrderQty = 1,
            ItemPrice = mealProduct.FinalPrice 
        });

        return order;
    }

    public Order CreateOrderWithPromo(List<MealSelectionDto> selectedDtos, int customerId, int? promoId = null)
    {
        var productItems = _mapper.Map<List<MealProductItem>>(selectedDtos);

        // Initialize MealProduct
        var mealProduct = new MealProduct
        {
            OwnerID = customerId,
            ProductName = $"Custom Package - {DateTime.Now:MM/dd/yyyy}",
            MealProductItems = productItems,
            // If the user provided a promo code, we link it here
            PromoID = promoId
        };

        // Logic Check: If PromoID is set, the Entity's FinalPrice property
        // will automatically call Promo.ApplyDiscount(ProductBasePrice).

        var order = new Order
        {
            RecipientID = customerId,
            OrderID = Guid.NewGuid(),
            DeliveryType = DeliveryStatus.OnCart,
            DeliveryStatus = DeliveryStatus.Pending
        };

        // The OrderTotalAmount in the Order model will reflect the 
        // MealProduct's FinalPrice because of this addition:
        order.OrderItems.Add(new OrderMealProduct
        {
            MealProduct = mealProduct,
            MealProductOrderQty = 1,
            // We use the computed FinalPrice which accounts for the Promo
            ItemPrice = mealProduct.FinalPrice
        });

        return order;
    }
}