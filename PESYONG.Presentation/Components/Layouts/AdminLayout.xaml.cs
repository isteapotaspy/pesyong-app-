using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PESYONG.Presentation.Interfaces;
using PESYONG.Presentation.Views.Admin;

namespace PESYONG.Presentation.Components.Layouts;

public sealed partial class AdminLayout : UserControl, ILayout
{
    public AdminLayout()
    {
        InitializeComponent();
        if (AdminLayoutNavView.SettingsItem is NavigationViewItem settingsItem)
        {
            settingsItem.Tag = "PESYONG.Presentation.Views.Admin.SettingsPage";
        }
    }

    public Frame ContentFrame => MainContentFrame;
    public void NavigateToPage(Type pageType)
    {
        ContentFrame.Navigate(pageType);
    }

    private void NavigationViewItem_Invoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        var tag = args.InvokedItemContainer?.Tag?.ToString();

        // Handle Settings item separately
        if (args.InvokedItemContainer == sender.SettingsItem)
        {
            NavigateToPage(typeof(PESYONG.Presentation.Views.Admin.SettingsPage));
        }
        else if (!string.IsNullOrEmpty(tag))
        {
            var pageType = Type.GetType(tag);
            NavigateToPage(pageType);
        }
    }
}
