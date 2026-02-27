using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace PESYONG.Presentation.ViewModels;
public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    private string _welcomeMessage;

    [ObservableProperty]
    private string _userName = "Valued Customer"; // You can pull this from a LoginService later

    public HomeViewModel()
    {
        int hour = DateTime.Now.Hour;
        _welcomeMessage = hour < 12 ? "Good Morning" : (hour < 18 ? "Good Afternoon" : "Good Evening");
    }

    [RelayCommand]
    private void StartNewOrder()
    {
        // We'll use this to navigate to the PackagesPage once we fix the crash
    }
}