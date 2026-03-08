using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PESYONG.ApplicationLogic.Services;
using PESYONG.Presentation.Interfaces;
using PESYONG.Presentation.Views.Customer;
using System;

namespace PESYONG.Presentation.Components.Layouts;

public sealed partial class CustomerLayout : UserControl, ILayout
{
    private bool _isInternalSelectionChange;
    private readonly CartService _cartService;

    public CustomerLayout()
    {
        InitializeComponent();

        _cartService = CartService.Instance;
        _cartService.CartUpdated += CartService_CartUpdated;

        MainNavigationViewFrame.Navigated += MainNavigationViewFrame_Navigated;
        Unloaded += CustomerLayout_Unloaded;

        UpdateCartBadge();

        ContentFrame.Navigate(typeof(HomePage));
    }

    public Frame ContentFrame => MainNavigationViewFrame;

    public void NavigateToPage(Type pageType)
    {
        if (ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }

    public void NavigateByTag(string tag)
    {
        Type? pageType = GetPageTypeFromTag(tag);
        if (pageType == null)
            return;

        if (ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }

        if (IsTopMenuTag(tag))
        {
            UpdateSelectedNavigationItem(tag);
        }
        else
        {
            ClearSelectedNavigationItem();
        }
    }

    private void CustomerLayout_Unloaded(object sender, RoutedEventArgs e)
    {
        _cartService.CartUpdated -= CartService_CartUpdated;
        MainNavigationViewFrame.Navigated -= MainNavigationViewFrame_Navigated;
        Unloaded -= CustomerLayout_Unloaded;
    }

    private void CartService_CartUpdated(object? sender, EventArgs e)
    {
        UpdateCartBadge();
    }

    private void UpdateCartBadge()
    {
        int count = _cartService.GetTotalItemCount();

        if (count > 0)
        {
            HeaderCartBadge.Visibility = Visibility.Visible;
            HeaderCartBadgeText.Text = count > 99 ? "99+" : count.ToString();
        }
        else
        {
            HeaderCartBadge.Visibility = Visibility.Collapsed;
            HeaderCartBadgeText.Text = string.Empty;
        }
    }

    private bool IsTopMenuTag(string tag)
    {
        return tag == "CateringPackagesPage"
            || tag == "ShortOrdersPage"
            || tag == "KakaninPage";
    }

    private Type? GetPageTypeFromTag(string tag)
    {
        return tag switch
        {
            "CateringPackagesPage" => typeof(CateringPackagesPage),
            "ShortOrdersPage" => typeof(ShortOrdersPage),
            "KakaninPage" => typeof(KakaninPage),
            "CartPage" => typeof(CartPage),
            "OrderHistoryPage" => typeof(OrderHistoryPage),
            "HelpPage" => typeof(HelpPage),
            "HomePage" => typeof(HomePage),
            _ => null
        };
    }

    private string? GetTopMenuTagFromPageType(Type pageType)
    {
        if (pageType == typeof(CateringPackagesPage))
            return "CateringPackagesPage";

        if (pageType == typeof(ShortOrdersPage))
            return "ShortOrdersPage";

        if (pageType == typeof(KakaninPage))
            return "KakaninPage";

        return null;
    }

    private void MainNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (_isInternalSelectionChange)
            return;

        if (args.SelectedItem is NavigationViewItem selectedItem &&
            selectedItem.Tag is string tag)
        {
            Type? pageType = GetPageTypeFromTag(tag);

            if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
            {
                ContentFrame.Navigate(pageType);
            }
        }
    }

    private void MainNavigationViewFrame_Navigated(object sender, NavigationEventArgs e)
    {
        if (e.SourcePageType == null)
            return;

        string? topMenuTag = GetTopMenuTagFromPageType(e.SourcePageType);

        if (!string.IsNullOrWhiteSpace(topMenuTag))
        {
            UpdateSelectedNavigationItem(topMenuTag);
        }
        else
        {
            ClearSelectedNavigationItem();
        }
    }

    private void UpdateSelectedNavigationItem(string tag)
    {
        _isInternalSelectionChange = true;

        try
        {
            foreach (object item in MainNavigationView.MenuItems)
            {
                if (item is NavigationViewItem navItem &&
                    navItem.Tag?.ToString() == tag)
                {
                    MainNavigationView.SelectedItem = navItem;
                    return;
                }
            }

            MainNavigationView.SelectedItem = null;
        }
        finally
        {
            _isInternalSelectionChange = false;
        }
    }

    private void ClearSelectedNavigationItem()
    {
        _isInternalSelectionChange = true;

        try
        {
            MainNavigationView.SelectedItem = null;
        }
        finally
        {
            _isInternalSelectionChange = false;
        }
    }

    private void CartButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateByTag("CartPage");
    }

    private void OrderHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateByTag("OrderHistoryPage");
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateByTag("HelpPage");
    }
}