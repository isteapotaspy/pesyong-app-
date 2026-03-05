using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Enums; // Required for DeliveryStatus
using System.Linq;

namespace PESYONG.Presentation.ViewModels;

/// <summary>
/// Prepares and finalizes order details before submission.
/// Maps UI-specific fields like shipping addresses and customer notes 
/// back to the <see cref="Order"/> domain entity.
/// </summary>
public partial class CheckoutViewModel : ObservableObject
{
    [ObservableProperty]
    private Order? _currentOrder;

    [ObservableProperty]
    private string _shippingAddress = string.Empty;

    [ObservableProperty]
    private string _notes = string.Empty;

    // This is called by the View's OnNavigatedTo
    /// <summary>
    /// Loads the existing order data into the checkout state.
    /// Usually called during navigation to pre-populate address and notes.
    /// </summary>
    public void Initialize(Order order)
    {
        CurrentOrder = order;
        // Pre-fill fields from the Domain Entity
        ShippingAddress = order.Address ?? string.Empty;
        Notes = order.CustomerNotes ?? string.Empty;
    }

    /// <summary>
    /// Synchronizes UI edits to the <see cref="Order"/> model and 
    /// updates the <see cref="DeliveryStatus"/> to Pending to begin processing.
    /// </summary>
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