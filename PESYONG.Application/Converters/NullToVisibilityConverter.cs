using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace PESYONG.ApplicationLogic.Converters
{
    /// <summary>
    /// Converts a null or empty value to Visibility.Visible or Visibility.Collapsed.
    /// Used to show/hide UI elements based on whether a value exists.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value to a Visibility enumeration.
        /// </summary>
        /// <param name="value">The value to check for null or emptiness.</param>
        /// <param name="targetType">The type of the target property (not used).</param>
        /// <param name="parameter">
        /// Optional parameter. If "inverse" is passed, the logic is reversed:
        /// show when null, hide when not null.
        /// </param>
        /// <param name="language">The language information (not used).</param>
        /// <returns>
        /// Visibility.Visible if the value is not null/empty (or if inverse and value is null),
        /// otherwise Visibility.Collapsed.
        /// </returns>
        /// 
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Check if we need to inverse the logic
            bool inverse = parameter?.ToString()?.ToLower() == "inverse";

            bool isNull = value == null || (value is string str && string.IsNullOrEmpty(str));

            if (inverse)
            {
                // If inverse, show when null, hide when not null
                return isNull ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                // Normal: hide when null, show when not null
                return isNull ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        /// <summary>
        /// Not implemented. Converts a Visibility back to a value.
        /// </summary>
        /// <param name="value">The Visibility value to convert back.</param>
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