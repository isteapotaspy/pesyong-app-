using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PESYONG.Presentation.Admin.Views;

/// <summary>
/// Interaction logic for LoginPage.xaml
/// </summary>
public partial class LoginPage : Page
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        // If the authentication was successful
        new MainAdminWindow().Show();
        Window.GetWindow(this)?.Close();
    }
}
