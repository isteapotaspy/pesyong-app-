using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PESYONG.Presentation.Interfaces;
using PESYONG.Presentation.Views.Customer;
using PESYONG.Presentation.Views.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.NetworkOperators;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PESYONG.Presentation.Components.Layouts;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class CustomerLayout : UserControl, ILayout
{
    public CustomerLayout()
    {
        this.InitializeComponent();

        // Optional: Navigate to default page on load
        // ContentFrame.Navigate(typeof(YourDefaultPage));
    }

    // ILayout implementation - now using the Frame inside NavigationView
    public Frame ContentFrame => MainNavigationViewFrame;

    public void NavigateToPage(Type pageType)
    {
        ContentFrame.Navigate(pageType);
    }

    // Event handler for NavigationView selection
    private void MainNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem selectedItem)
        {
            string tag = selectedItem.Tag.ToString();

            switch (tag)
            {
                case "CateringPackagesPage":
                    ContentFrame.Navigate(typeof(CateringPackagesPage));
                    break;
                case "ShortOrdersPage":
                    ContentFrame.Navigate(typeof(ShortOrdersPage));
                    break;
                case "KakaninPage":
                    ContentFrame.Navigate(typeof(KakaninPage));
                    break;
                case "CartPage":
                    ContentFrame.Navigate(typeof(CartPage));
                    break;
                case "OrderHistoryPage":
                    ContentFrame.Navigate(typeof(OrderHistoryPage));
                    break;
                case "HelpPage":
                    ContentFrame.Navigate(typeof(HelpPage));
                    break;
                default:
                    // Handle unknown navigation
                    break;
            }
        }
    }

    // Optional: Profile button click handler
    private void ProfileButton_Click(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(ProfilePage));
    }
}