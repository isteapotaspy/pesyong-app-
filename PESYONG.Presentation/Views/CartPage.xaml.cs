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

namespace PESYONG.Presentation.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class CartPage : Page
{
    public CartViewModel ViewModel { get; }
    public CartPage()
    {
        this.ViewModel = App.Current.Services.GetRequiredService<CartViewModel>();


        this.InitializeComponent();
    }

    /// <summary>
    /// Optional: Navigation logic to load the user's specific cart 
    /// when they navigate to this page.
    /// </summary>
    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // If passing the Order or User as a parameter
        if (e.Parameter is Order existingOrder)
        {
            ViewModel.CurrentOrder = existingOrder;
        }
    }
}
