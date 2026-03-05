using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections;

namespace PESYONG.ApplicationLogic.Converters
{
    /// <summary>
    /// Converts the count of a collection to Visibility.Visible or Visibility.Collapsed.
    /// Used to show/hide UI elements based on whether a collection has items.
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a collection count to a Visibility enumeration.
        /// </summary>
        /// <param name="value">The collection or count value to evaluate.</param>
        /// <param name="targetType">The type of the target property (not used).</param>
        /// <param name="parameter">
        /// Optional parameter. If provided, shows Visibility.Visible only when count equals this parameter value.
        /// Without parameter, shows Visibility.Visible when count > 0.
        /// </param>
        /// <param name="language">The language information (not used).</param>
        /// <returns>
        /// Visibility.Visible if the count meets the criteria (count > 0 or count equals parameter),
        /// otherwise Visibility.Collapsed.
        /// </returns>
        /// 

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return Visibility.Collapsed;

            int count = 0;

            // Handle different collection types
            if (value is ICollection collection)
            {
                count = collection.Count;
            }
            else if (value is int intValue)
            {
                count = intValue;
            }
            else
            {
                return Visibility.Collapsed;
            }

            // Check if we need to compare with a parameter
            if (parameter != null)
            {
                if (int.TryParse(parameter.ToString(), out int compareValue))
                {
                    // Show if count equals compareValue
                    return count == compareValue ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            // Default: show if count > 0
            return count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Not implemented. Converts a Visibility back to a count value.
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