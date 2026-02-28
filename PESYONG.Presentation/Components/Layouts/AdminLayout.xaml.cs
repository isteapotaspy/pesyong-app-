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
        this.InitializeComponent();
    }

    public Frame ContentFrame => MainContentFrame;
    public void NavigateToPage(Type pageType)
    {
        ContentFrame.Navigate(pageType);
    }

    private void NavigationViewItem_Invoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        var pageType = Type.GetType(args.InvokedItemContainer.Tag.ToString());
        try
        {
            if (pageType != null)
            {
                NavigateToPage(pageType);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
