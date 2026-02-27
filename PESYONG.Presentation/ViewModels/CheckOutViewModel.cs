using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Enums; // Required for DeliveryStatus
using System.Linq;

namespace PESYONG.Presentation.ViewModels;

public partial class CheckoutViewModel : ObservableObject
{
    [ObservableProperty]
    private Order? _currentOrder;

    [ObservableProperty]
    private string _shippingAddress = string.Empty;

    [ObservableProperty]
    private string _notes = string.Empty;

    // This is called by the View's OnNavigatedTo
    public void Initialize(Order order)
    {
        CurrentOrder = order;
        // Pre-fill fields from the Domain Entity
        ShippingAddress = order.Address ?? string.Empty;
        Notes = order.CustomerNotes ?? string.Empty;
    }

    [RelayCommand]
    private void PlaceOrder()
    {
        if (CurrentOrder == null) return;

        // Apply UI values back to your Domain Model properties
        CurrentOrder.Address = ShippingAddress;
        CurrentOrder.CustomerNotes = Notes;
        
        // Update status using your Domain Enum
        CurrentOrder.DeliveryStatus = DeliveryStatus.Pending;
        CurrentOrder.DeliveryType = DeliveryStatus.OnCart; // Or your logic for checkout

        // TODO: Call your Repository/Service to save to DB
    }
}