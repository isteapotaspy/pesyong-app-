using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PESYONG.Presentation.Interfaces;

namespace PESYONG.Presentation.Components.Layouts;

public sealed partial class AdminLayout : UserControl, ILayout
{
    public static readonly DependencyProperty TitleTextProperty =
        DependencyProperty.Register(nameof(TitleText), typeof(string), typeof(AdminLayout), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty SubtitleTextProperty =
        DependencyProperty.Register(nameof(SubtitleText), typeof(string), typeof(AdminLayout), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ShowBackButtonSettingProperty =
        DependencyProperty.Register(nameof(ShowBackButtonSetting), typeof(bool), typeof(AdminLayout), new PropertyMetadata(true));

    public static readonly DependencyProperty ShowPaneButtonSettingProperty =
        DependencyProperty.Register(nameof(ShowPaneButtonSetting), typeof(bool), typeof(AdminLayout), new PropertyMetadata(true));

    public AdminLayout()
    {
        this.InitializeComponent();
    }

    public string TitleText
    {
        get { return (string)GetValue(TitleTextProperty); }
        set { SetValue(TitleTextProperty, value); }
    }

    public string SubtitleText
    {
        get { return (string)GetValue(SubtitleTextProperty); }
        set { SetValue(SubtitleTextProperty, value); }
    }

    public bool ShowBackButtonSetting
    {
        get { return (bool)GetValue(ShowBackButtonSettingProperty); }
        set { SetValue(ShowBackButtonSettingProperty, value); }
    }

    public bool ShowPaneButtonSetting
    {
        get { return (bool)GetValue(ShowPaneButtonSettingProperty); }
        set { SetValue(ShowPaneButtonSettingProperty, value); }
    }

    public Frame ContentFrame => MainContentFrame;

    public void NavigateToPage(Type pageType)
    {
        ContentFrame.Navigate(pageType);
    }
}