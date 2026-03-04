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

// Converts boolean availability to a SolidColorBrush (green/red)
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

// Converts boolean availability into button background brush (orange when available, gray when not)
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

// Converts boolean availability into button foreground brush (white when available, dark gray when not)
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

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

// Converts a boolean availability into the actual Style object from Application resources
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
