using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Converts a <see cref="DateTime"/> object into a formatted string 
/// (e.g., "January 1, 2026") for UI display.
/// </summary>
namespace PESYONG.ApplicationLogic.Converters
{
    public class DateTimeToStringConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dateTime)
            {
                return dateTime.ToString("MMMM d, yyyy");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
