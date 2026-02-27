using System;
using Microsoft.UI.Xaml.Controls;


namespace PESYONG.Presentation.Interfaces;

/// <summary>
/// This is the base interface for both the admin and the customer layout.
/// It ensures that both can be easily switched with each other even if
/// they are both of different classes, and of different namespaces.
/// </summary>

public interface ILayout
{
    Frame ContentFrame { get; }
    void NavigateToPage(Type pageType);
}
