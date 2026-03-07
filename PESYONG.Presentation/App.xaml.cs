using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using PESYONG.ApplicationLogic.Mapping;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.ApplicationLogic.Services;
using PESYONG.ApplicationLogic.ViewModels.ObjectModels;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Users.Identity;
using PESYONG.Infrastructure;
using PESYONG.Presentation.Profiler;
using PESYONG.Presentation.ViewModels;
using PESYONG.Presentation.ViewModels.ObjectModels;
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
    public static Window MainWindow { get; private set; }
    public IServiceProvider Services { get; }

    private static App _instance;
    public static App Instance => _instance ??= (App)Current;
    public AppUser CurrentUser { get; set; }

    private IHost _host;


    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();

        var services = new ServiceCollection();
        services.AddAutoMapper(
            typeof(CateringMappingProfile).Assembly,
            typeof(MealMappingProfile).Assembly
            );

        var connectionString =
                  @"Server=localhost\SQLEXPRESS;Database=PesyongDb;Trusted_Connection=True;TrustServerCertificate=True;";

        services.AddDbContextFactory<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        // To be implemented, auth services
        services.AddSingleton<AuthenticationService>();
        services.AddSingleton<AuthorizationService>();


        // Register repository accessors
        services.AddScoped<AcknowledgementReceiptRepository>();
        services.AddScoped<AppUserRepository>();
        services.AddScoped<AuditLogRepository>();
        services.AddScoped<CustomerRepository>();
        services.AddScoped<DeliveryRepository>();
        services.AddScoped<DeliveryUpdateRepository>();
        services.AddScoped<MealProductRepository>();
        services.AddScoped<MealRepository>();
        services.AddScoped<OrderRepository>();
        services.AddScoped<PaymentRepository>();
        services.AddScoped<PromoRepository>();

        // Register services
        services.AddScoped<AcknowledgementReceiptService>();
        services.AddScoped<CateringService>();
        services.AddSingleton<MealSyncService>();

        // Customer ViewModels
        services.AddTransient<PackagesViewModel>();
        services.AddTransient<CheckoutViewModel>();
        services.AddTransient<HomeViewModel>();

        // Admin ViewModels
        // THIS IS WHY WE FREAKING USE DEPENDENCY INJECTION 
        // I LITERALLY SAID TO LEARN THIS JUD BA UNYA WA GINA TAKE SERIOUSLY ISTG
        services.AddTransient<AcknowledgementReceiptViewModel>();
        services.AddTransient<AppUserViewModel>();
        services.AddTransient<AuditLogViewModel>();
        services.AddTransient<CustomerViewModel>();
        services.AddTransient<DeliveryViewModel>();
        services.AddTransient<DeliveryUpdateViewModel>();
        services.AddTransient<MealProductViewModel>();
        services.AddTransient<MealProductItemViewModel>();
        services.AddTransient<MealViewModel>();
        services.AddTransient<OrderMealProductViewModel>();
        services.AddTransient<OrderViewModel>();

        // Not implemented as view models yet
        //services.AddTransient<PaymentRepository>();
        //services.AddTransient<PromoRepository>();



        Services = services.BuildServiceProvider();

        TestDependencyInjection();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _instance = this;
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }

    // Move this to proper testing suite later on
    private void TestDependencyInjection()
    {
        try
        {
            Debug.WriteLine("\n\n=== Testing Dependency Injection ===");
            var appDb = Services.GetService<AppDbContext>();
            Debug.WriteLine($"MealRepository: {(appDb != null ? "[/] Resolved" : "[X] Failed")}");

            var mealRepo = Services.GetService<MealRepository>();
            Debug.WriteLine($"MealRepository: {(mealRepo != null ? "[/] Resolved" : "[X] Failed")}");

            var mealVM = Services.GetService<MealViewModel>();
            Debug.WriteLine($"MealViewModel: {(mealVM != null ? "[/] Resolved" : "[X] Failed")}");

            var dbContext = Services.GetService<AppDbContext>();
            Debug.WriteLine($"DbContext: {(dbContext != null ? "[/] Resolved" : "[X] Failed")}");

            Debug.WriteLine("=== Dependency Injection Test Complete ===\n");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Dependency Injection Test Failed: {ex.Message}\n");
        }
    }


    // Helper to access App.Current.Services easily
    public static new App Current => (App)Microsoft.UI.Xaml.Application.Current;

    public object DispatcherQueue { get; internal set; }
}
