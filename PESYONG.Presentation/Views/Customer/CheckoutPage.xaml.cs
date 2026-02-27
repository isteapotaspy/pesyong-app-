using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
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

        // This is where the 'Order' object from PackagesPage arrives
        if (e.Parameter is Order order)
        {
            ViewModel.Initialize(order);
        }
        else
        {
            // Safety: If no data was passed, go back to prevent null errors
            if (this.Frame != null && this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
    }
}
