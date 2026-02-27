using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PESYONG.Presentation.Views
{
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
}
