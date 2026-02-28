using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PESYONG.Presentation.Interfaces;
using PESYONG.Presentation.Views.Admin;

namespace PESYONG.Presentation.Components.Layouts
{
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



    }
}
