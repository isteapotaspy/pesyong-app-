
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using PESYONG.Presentation.Interfaces;
using System;
using Windows.System;
using WinRT.Interop;
using System.Runtime.InteropServices;

namespace PESYONG.Presentation;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// This contains the authorization logic of our application.
/// @Jane @Shayne: Change isAdmin:false to go back 
/// </summary>

/// <remarks>
/// TASK: Implement context switching between layouts.
/// TASK: Implement the Login logic later on.
/// </remarks>

/// Why is Loaded undefined here?
/// Likewise, _currentLayout should be set at minimum permitted authorization instead,
/// which is for user kiosk on default so that this doesn't throw an error. 
/// AdminLayout() and CustomerLayout() are also likewise not defined here.

public sealed partial class MainWindow : Window
{
    private ILayout _currentLayout;

    public MainWindow()
    {
        InitializeComponent();
        // This will determine what layout your user goes.
        SetLayoutBasedOnUserRole(isAdmin: false);
        this.Content.KeyDown += OnKeyDown;
    }

    public void SetLayoutBasedOnUserRole(bool isAdmin)
    {
        // This is to which layout the program will go,
        // either administrator or customer kiosk.
        _currentLayout = null;

        // This is to which content/page the program will go,
        // assuming that it's within the layout you've defined
        // in _currentLayout.
        LayoutContentControl.Content = null;

        // Authorization logic is defined here.
        if (isAdmin)
        {
            var adminLayout = new Components.Layouts.AdminLayout();
            LayoutContentControl.Content = adminLayout;
            _currentLayout = adminLayout;

            adminLayout.NavigateToPage(typeof(Views.Admin.DashboardPage));
        }
        else
        {
            var customerLayout = new Components.Layouts.CustomerLayout();
            LayoutContentControl.Content = customerLayout;
            _currentLayout = customerLayout;

            customerLayout.NavigateToPage(typeof(Views.Customer.HomePage));
        }
    }

    public void SwitchToAdminLayout()
    {
        SetLayoutBasedOnUserRole(isAdmin: true);
    }

    public void SwitchToCustomerLayout()
    {
        SetLayoutBasedOnUserRole(isAdmin: false);
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            // F11 may be intercepted by Visual Studio / host environment,
            // so F12 is kept as a fallback during development.
            case Windows.System.VirtualKey.F11: // intended kiosk key
            case Windows.System.VirtualKey.F12: // dev fallback
                ToggleFullscreen();
                e.Handled = true;
                break;
        }
    }

    private void ToggleFullscreen()
    {
        var presenter = this.AppWindow.Presenter;

        if (presenter.Kind == Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen)
        {
            this.AppWindow.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.Overlapped);
        }
        else
        {
            this.AppWindow.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen);
        }
    }
}

// IMPLEMENT THIS ON LOGIN PAGE
// private void OnLoginSuccessful(bool isAdminUser)
// {  
//     (Application.Current as App)?.MainWindow?.SetLayoutBasedOnUserRole(isAdminUser);
// }
