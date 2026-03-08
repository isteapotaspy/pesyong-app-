using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PESYONG.Presentation.ViewModels.ObjectModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PESYONG.Presentation.Views.Admin.Administration;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class CustomerPage : Page
{

    private ObservableCollection<CustomerViewModel> CustomerListViewModel;
    public CustomerPage()
    {
        InitializeComponent();
    }


    private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    { }

    private async void AddCustomerButton_Click(object sender, RoutedEventArgs e)
    { }

    private void ShowQueryPopupButton_Click(object sender, RoutedEventArgs e)
    {
        if (!QueryPopup.IsOpen)
        {
            QueryPopup.IsOpen = true;
        }
    }

    private void CloseQueryPopupButton_Click(object sender, RoutedEventArgs e)
    {
        if (QueryPopup.IsOpen)
        {
            QueryPopup.IsOpen = false;
        }
    }

}
