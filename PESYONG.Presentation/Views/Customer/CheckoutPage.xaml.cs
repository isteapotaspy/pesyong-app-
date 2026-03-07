using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PESYONG.ApplicationLogic.Services;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Presentation.ViewModels;

namespace PESYONG.Presentation.Views.Customer;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class CheckoutPage : Page
{
    public CheckoutViewModel ViewModel { get; }

    public CheckoutPage()
    {
        this.ViewModel = App.Current.Services.GetRequiredService<CheckoutViewModel>();
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        var cart = CartService.Instance.Cart;

        if (cart != null && cart.Count > 0)
        {
            ViewModel.Initialize(cart);
        }
        else
        {
            if (this.Frame != null && this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
    }
}