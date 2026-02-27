using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.Infrastructure;
using PESYONG.Presentation.ViewModels;
using PESYONG.ApplicationLogic.Mapping;
using PESYONG.ApplicationLogic.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PESYONG.Presentation;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Microsoft.UI.Xaml.Application
{
    public Window MainWindow { get; private set; }
    public IServiceProvider Services { get; }


    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();

        var services = new ServiceCollection();
        services.AddAutoMapper(typeof(CateringMappingProfile));
        services.AddSingleton<CateringService>();

        // Register ViewModel
        services.AddTransient<PackagesViewModel>();
        services.AddTransient<CheckoutViewModel>();
        services.AddTransient<HomeViewModel>();
        //services.AddDbContext<AppDbContext>(options =>
        //options.UseSqlite("Data Source=pesyong.db"));

        services.AddScoped<OrderRepository>();
        services.AddScoped<OrderRepository>();
        Services = services.BuildServiceProvider();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }

    // Helper to access App.Current.Services easily
    public static new App Current => (App)Microsoft.UI.Xaml.Application.Current;
}
