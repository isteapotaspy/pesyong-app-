using AutoMapper;
using PESYONG.ApplicationLogic.DTOs;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Meals.MealProduct;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PESYONG.ApplicationLogic.Services;

public class CateringService
{
    private readonly IMapper _mapper;

    public CateringService(IMapper mapper)
    {
        _mapper = mapper;
    }

    public List<MealSelectionDto> GetAvailableViands()
    {
        // Temporary/mock data
        var meals = new List<Meal>
        {
            new() { MealID = 1, MealName = "Lechon Kawali", MealPrice = 150.00m },
            new() { MealID = 2, MealName = "Pork Adobo", MealPrice = 120.00m },
            new() { MealID = 3, MealName = "Beef Caldereta", MealPrice = 180.00m }
        };

        return _mapper.Map<List<MealSelectionDto>>(meals);
    }

    public Order CreateOrderFromSelection(
        List<MealSelectionDto> selectedDtos,
        Guid customerId,
        int? ownerId = null)
    {
        ValidateSelection(selectedDtos);

        var productItems = _mapper.Map<List<MealProductItem>>(selectedDtos);

        var mealProduct = new MealProduct
        {
            OwnerID = ownerId,
            ProductName = $"Custom Package - {DateTime.Now:MM/dd/yyyy}",
            IsCateringPackage = true,
            MealProductItems = productItems
        };

        ValidateMealProduct(mealProduct);

        var order = new Order
        {
            OrderID = Guid.NewGuid(),
            CustomerID = customerId,
            DeliveryType = DeliveryStatus.OnCart,
            DeliveryStatus = DeliveryStatus.Pending
        };

        order.OrderItems.Add(new OrderMealProduct
        {
            OrderID = order.OrderID,
            MealProduct = mealProduct,
            MealProductOrderQty = 1,
            ItemPrice = mealProduct.FinalPrice
        });

        return order;
    }

    public Order CreateOrderWithPromo(
        List<MealSelectionDto> selectedDtos,
        Guid customerId,
        int? ownerId = null,
        int? promoId = null)
    {
        ValidateSelection(selectedDtos);

        var productItems = _mapper.Map<List<MealProductItem>>(selectedDtos);

        var mealProduct = new MealProduct
        {
            OwnerID = ownerId,
            ProductName = $"Custom Package - {DateTime.Now:MM/dd/yyyy}",
            IsCateringPackage = true,
            MealProductItems = productItems,
            PromoID = promoId
        };

        ValidateMealProduct(mealProduct);

        var order = new Order
        {
            OrderID = Guid.NewGuid(),
            CustomerID = customerId,
            DeliveryType = DeliveryStatus.OnCart,
            DeliveryStatus = DeliveryStatus.Pending
        };

        order.OrderItems.Add(new OrderMealProduct
        {
            OrderID = order.OrderID,
            MealProduct = mealProduct,
            MealProductOrderQty = 1,
            ItemPrice = mealProduct.FinalPrice
        });

        return order;
    }

    private static void ValidateSelection(List<MealSelectionDto> selectedDtos)
    {
        if (selectedDtos == null || selectedDtos.Count == 0)
            throw new InvalidOperationException("Please select at least one viand.");

        if (selectedDtos.Count > 8)
            throw new InvalidOperationException("A custom package cannot exceed 8 viands.");
    }

    private static void ValidateMealProduct(MealProduct mealProduct)
    {
        if (!mealProduct.IsValid())
        {
            throw new InvalidOperationException(
                string.Join(Environment.NewLine, mealProduct.GetValidationErrors()));
        }
    }
}