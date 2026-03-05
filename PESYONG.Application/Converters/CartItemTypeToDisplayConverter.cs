using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Converts raw cart item type strings (e.g., "package", "kakanin") into 
/// human-readable display names for the UI.
/// </summary>

namespace PESYONG.ApplicationLogic.Converters
{
    public class CartItemTypeToDisplayConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string type)
            {
                return type switch
                {
                    "package" => "Catering Package",
                    "short-order" => "Short Order",
                    "kakanin" => "Kakanin",
                    _ => type
                };
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
