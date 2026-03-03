using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PESYONG.Presentation.Interfaces;
using PESYONG.Presentation.Views.Admin;
using Microsoft.Extensions.DependencyInjection;

namespace PESYONG.Presentation.Components.Layouts;

public sealed partial class AdminLayout : UserControl, ILayout
{
    private Type _pendingPageType;
    private bool _isLoaded;

    public AdminLayout()
    {
        InitializeComponent();
        if (AdminLayoutNavView.SettingsItem is NavigationViewItem settingsItem)
        {
            settingsItem.Tag = "PESYONG.Presentation.Views.Admin.SettingsPage";
        }

        Loaded += AdminLayout_Loaded;
    }

    public Frame ContentFrame => MainContentFrame;

    private void AdminLayout_Loaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = true;
        if (_pendingPageType != null)
        {
            var pageToNavigate = _pendingPageType;
            _pendingPageType = null;
            NavigateToPage(pageToNavigate);
        }
    }

    public void NavigateToPage(Type pageType)
    {
        try
        {
            if (ContentFrame == null)
                return;

            if (!_isLoaded)
            {
                _pendingPageType = pageType;
                return;
            }

            if (ContentFrame.Dispatcher != null)
            {
                _ = ContentFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    if (ContentFrame != null)
                    {
                        NavigateWithDependencyInjection(pageType);
                    }
                });
            }
            else
            {
                NavigateWithDependencyInjection(pageType);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("The Admin Page failed to mount, or it failed to navigate. " + ex);
        }
    }

    private void NavigateWithDependencyInjection(Type pageType)
    {
        try
        {
            var serviceProvider = (Microsoft.UI.Xaml.Application.Current as App)?.Services;
            if (serviceProvider != null)
            {
                var page = ActivatorUtilities.CreateInstance(serviceProvider, pageType) as Page;
                if (page != null)
                {
                    ContentFrame.Content = page;
                }
                else
                {
                    ContentFrame.Navigate(pageType);
                }
            }
            else
            {
                ContentFrame.Navigate(pageType);
            }
        }
        catch
        {
            ContentFrame.Navigate(pageType);
        }

        // Handle null from downstream to upstream
    }

    private void NavigationViewItem_Invoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        var tag = args.InvokedItemContainer?.Tag?.ToString();

        if (args.InvokedItemContainer == sender.SettingsItem)
        {
            NavigateToPage(typeof(PESYONG.Presentation.Views.Admin.SettingsPage));
        }
        else if (tag == "PESYONG.Presentation.Views.Admin.Meals.MealPage")
        {
            NavigateToPage(typeof(PESYONG.Presentation.Views.Admin.Meals.MealPage));
        }
        else if (!string.IsNullOrEmpty(tag))
        {
            var pageType = Type.GetType(tag);
            if (pageType != null)
            {
                NavigateToPage(pageType);
            }
        }
    }
}