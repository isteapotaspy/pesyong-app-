using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// CONVERTER: GreaterThanZeroToVisibilityConverter
/// NAMESPACE: PESYONG.Presentation.Converters
/// 
/// SUMMARY:
/// Value converter that converts an integer to Visibility enum.
/// Returns Visible if the value is greater than 0, otherwise Collapsed.
/// Used primarily for showing/hiding cart badges on short order cards.
/// 
/// PURPOSE:
/// Provides a clean way to conditionally display UI elements based on
/// numeric values directly in XAML without code-behind logic.
/// 
/// CONVERSION LOGIC:
/// - Input: integer value
/// - If value > 0 → returns Visibility.Visible
/// - If value ≤ 0 → returns Visibility.Collapsed
/// - If value is not an integer → returns Visibility.Collapsed (safe fallback)
/// 
/// XAML USAGE:
/// 1. Add namespace:
///    xmlns:converters="using:PESYONG.Presentation.Converters"
/// 
/// 2. Add to resources:
///    <converters:GreaterThanZeroToVisibilityConverter x:Key="GreaterThanZeroToVisibilityConverter"/>
/// 
/// 3. Use in binding:
///    Visibility="{Binding CartQuantity, Converter={StaticResource GreaterThanZeroToVisibilityConverter}}"
/// 
/// EXAMPLE:
/// <Border Visibility="{Binding CartQuantity, 
///                          Converter={StaticResource GreaterThanZeroToVisibilityConverter}}">
///     <TextBlock Text="{Binding CartQuantity} in cart"/>
/// </Border>
/// 
/// NOTES:
/// - One-way converter only (ConvertBack not implemented)
/// - Returns Collapsed for null or non-integer values
/// - Commonly used with cart badges, notification indicators, etc.
/// </summary>
/// 

namespace PESYONG.ApplicationLogic.Converters
{
    public class GreaterThanZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int intValue)
            {
                return intValue > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
