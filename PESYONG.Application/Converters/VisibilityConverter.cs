using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.Converters
{

    /// <summary>
    /// A multi-type converter that evaluates truthiness to return <see cref="Visibility"/>.
    /// <list type="bullet">
    /// <item>Booleans: True = Visible.</item>
    /// <item>Integers: Greater than 0 = Visible.</item>
    /// <item>Strings: Not null or empty = Visible.</item>
    /// <item>Objects: Not null = Visible.</item>
    /// </list>
    /// </summary>
    /// 
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            if (value is int intValue)
            {
                return intValue > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            if (value is string strValue)
            {
                return !string.IsNullOrEmpty(strValue) ? Visibility.Visible : Visibility.Collapsed;
            }
            if (value == null)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }
}
