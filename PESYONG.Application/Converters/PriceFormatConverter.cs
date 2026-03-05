using Microsoft.UI.Xaml.Data;
using System;

namespace PESYONG.ApplicationLogic.Converters
{
    /// <summary>
    /// Converts a numeric price value to a formatted string with currency symbol and thousand separators.
    /// </summary>
    public class PriceFormatConverter : IValueConverter
    {
        /// <summary>
        /// Converts a numeric value to a formatted price string (e.g., 1200 -> "₱1,200").
        /// </summary>
        /// <param name="value">The numeric value to format (decimal, int, or double).</param>
        /// <param name="targetType">The type of the target property (not used).</param>
        /// <param name="parameter">
        /// Optional parameter. Can be used to specify format:
        /// - "N0" (default): Format with thousand separators, no decimals
        /// - "N2": Format with thousand separators and 2 decimal places
        /// - "C": Format as currency (uses system locale)
        /// </param>
        /// <param name="language">The language information (not used).</param>
        /// <returns>A formatted price string with ₱ symbol.</returns>
        /// 

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return "₱0";

            decimal price = 0;

            // Handle different numeric types
            if (value is decimal decimalValue)
            {
                price = decimalValue;
            }
            else if (value is int intValue)
            {
                price = intValue;
            }
            else if (value is double doubleValue)
            {
                price = (decimal)doubleValue;
            }
            else if (value is float floatValue)
            {
                price = (decimal)floatValue;
            }
            else if (value is long longValue)
            {
                price = longValue;
            }
            else
            {
                return "₱0";
            }

            // Determine format based on parameter
            string format = parameter?.ToString() ?? "N0";

            // Apply the format
            return $"₱{price.ToString(format)}";
        }

        /// <summary>
        /// Not implemented. Converts a formatted price string back to a numeric value.
        /// </summary>
        /// <param name="value">The formatted price string to convert back.</param>
        /// <param name="targetType">The type of the target property.</param>
        /// <param name="parameter">Optional parameter.</param>
        /// <param name="language">The language information.</param>
        /// <returns>Throws NotImplementedException as this conversion is not supported.</returns>
        /// <exception cref="NotImplementedException">Always thrown as this method is not implemented.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}