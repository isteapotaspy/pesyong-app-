using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace PESYONG.ApplicationLogic.Converters;


/// <summary>
/// Converts a boolean availability status into a <see cref="SolidColorBrush"/>.
/// Returns Green (#4CAF50) for available and Red (#F44336) for out of stock.
/// </summary>

public class StockStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isAvailable = value is bool b && b;
        // Use explicit ARGB values to avoid returning a raw string which can cause binding/runtime errors
        var color = isAvailable ? Color.FromArgb(0xFF, 0x4C, 0xAF, 0x50) /* #4CAF50 */
                                : Color.FromArgb(0xFF, 0xF4, 0x43, 0x36) /* #F44336 */;
        return new SolidColorBrush(color);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Determines the background <see cref="SolidColorBrush"/> for buttons based on availability.
    /// Returns Brand Orange (#FF6600) if available, otherwise Light Gray (#CCCCCC).
    /// </summary>
    public class AvailableToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isAvailable = value is bool b && b;
        var color = isAvailable ? Color.FromArgb(0xFF, 0xFF, 0x66, 0x00) /* #FF6600 */
                                : Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC) /* #CCCCCC */;
        return new SolidColorBrush(color);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}


    /// <summary>
    /// Determines the text/icon <see cref="SolidColorBrush"/> for buttons based on availability.
    /// Returns White for available items and Dark Gray for disabled/out-of-stock items.
    /// </summary>
    public class AvailableToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isAvailable = value is bool b && b;
        var color = isAvailable ? Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF) /* #FFFFFF */
                                : Color.FromArgb(0xFF, 0x66, 0x66, 0x66) /* #666666 */;
        return new SolidColorBrush(color);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

 
}

/// <summary>
/// Dynamically switches between defined XAML Styles based on product availability.
/// Looks for 'AddToCartButtonStyle' or 'OutOfStockButtonStyle' in the Application Resources.
/// </summary>
public class AvailableToButtonStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string key = (value is bool b && b) ? "AddToCartButtonStyle" : "OutOfStockButtonStyle";
        try
        {
            var app = global::Microsoft.UI.Xaml.Application.Current;
            if (app != null && app.Resources != null && app.Resources.ContainsKey(key))
            {
                return app.Resources[key] as Style;
            }
        }
        catch
        {
            // swallow and fall through to return null so XAML can use defaults
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
