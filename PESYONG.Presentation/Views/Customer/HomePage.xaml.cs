using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PESYONG.Presentation.Components.Layouts;
using PESYONG.Presentation.ViewModels;

namespace PESYONG.Presentation.Views.Customer;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel { get; }

    public HomePage()
    {
        ViewModel = App.Current.Services.GetRequiredService<HomeViewModel>();
        InitializeComponent();
    }

    private void ViewPackagesButton_Click(object sender, RoutedEventArgs e)
    {
        var layout = FindParent<CustomerLayout>(this);
        layout?.NavigateByTag("CateringPackagesPage");
    }

    private void ExploreCateringButton_Click(object sender, RoutedEventArgs e)
    {
        var layout = FindParent<CustomerLayout>(this);
        layout?.NavigateByTag("CateringPackagesPage");
    }

    private void ExploreShortOrdersButton_Click(object sender, RoutedEventArgs e)
    {
        var layout = FindParent<CustomerLayout>(this);
        layout?.NavigateByTag("ShortOrdersPage");
    }

    private void ExploreKakaninButton_Click(object sender, RoutedEventArgs e)
    {
        var layout = FindParent<CustomerLayout>(this);
        layout?.NavigateByTag("KakaninPage");
    }

    private T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        DependencyObject parent = VisualTreeHelper.GetParent(child);

        while (parent != null)
        {
            if (parent is T typedParent)
                return typedParent;

            parent = VisualTreeHelper.GetParent(parent);
        }

        return null;
    }
}