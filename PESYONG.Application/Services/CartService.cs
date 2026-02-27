
using PESYONG.Domain.Entities;
using PESYONG.Domain.Entities.Meals.MealProduct;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Entities.Users.Identity;
using PESYONG.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PESYONG.Service.Services;

public class CartService
{
    // ObservableCollection automatically updates the WinUI UI when items are added/removed
    public ObservableCollection<CartItem> Cart { get; private set; } = new();
    public DeliveryInfo? Delivery { get; private set; }

    private Order _activeOrder;
    private Order _activeCart;
    private readonly AppUser _currentUser;

    public CartService(AppUser user)
    {
        _currentUser = user;
        // Find existing cart or create new
        _activeOrder = user.UserOrders.FirstOrDefault(o => o.DeliveryStatus == DeliveryStatus.OnCart)
                       ?? new Order { RecipientID = user.Id, DeliveryStatus = DeliveryStatus.OnCart };

    }

    public Order InitializeCart(AppUser user)
    {
        // Search user's orders for one still "OnCart"
        _activeCart = user.UserOrders.FirstOrDefault(o => o.DeliveryStatus == DeliveryStatus.OnCart);

        if (_activeCart == null)
        {
            _activeCart = new Order
            {
                OrderID = Guid.NewGuid(), // Order gets a Guid
                RecipientID = user.Id,    // User gets an int (from IdentityUser<int>)
                DeliveryStatus = DeliveryStatus.OnCart
            };
        }
        return _activeCart;
    }
    private List<CartItem> _cartItems = new();

    public void AddToCart(int productId, decimal price, int quantity = 1)
    {
        var existingItem = _cartItems.FirstOrDefault(x => x.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            _cartItems.Add(new CartItem
            {
                ProductId = productId,
                Price = (double)price,
                Quantity = quantity
            });
        }
    }

    public void RemoveFromCart(string id)
    {
        var item = Cart.FirstOrDefault(i => i.Id == id);
        if (item != null) Cart.Remove(item);
    }

    public void UpdateQuantity(string id, int quantity)
    {
        if (quantity <= 0)
        {
            RemoveFromCart(id);
            return;
        }

        var item = Cart.FirstOrDefault(i => i.Id == id);
        if (item != null) item.Quantity = quantity;
    }

    public void SetDelivery(DeliveryInfo info) => Delivery = info;

    public void ClearCart()
    {
        Cart.Clear();
        Delivery = null;
    }

    public double GetSubtotal() => Cart.Sum(item => item.Price * item.Quantity);

    public double GetTotal()
    {
        double subtotal = GetSubtotal();
        double fee = Delivery?.DeliveryFee ?? 0;
        return subtotal + fee;
    }
}