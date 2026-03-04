using Microsoft.UI.Xaml;
using PESYONG.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.Converters
{
    /// <summary>
    /// Converts <see cref="DeliveryStatus"/> to a Hex color string. 
    /// If a step index is provided as a parameter, it returns a gray color for future steps 
    /// to support a progress-bar visual style.
    /// </summary>
    public class OrderStatusToColorConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DeliveryStatus status)
            {
                // If parameter is provided, check if this step should be active
                if (parameter is string stepIndex && int.TryParse(stepIndex, out int step))
                {
                    int statusOrder = status switch
                    {
                        DeliveryStatus.Pending => 0,
                        DeliveryStatus.Preparing => 1,
                        DeliveryStatus.OutForDelivery => 2,
                        DeliveryStatus.Delivered => 3,
                        _ => -1
                    };

                    if (step > statusOrder)
                        return "#E5E7EB"; // Gray for incomplete steps
                }

                return status switch
                {
                    DeliveryStatus.Pending => "#F59E0B", // Yellow
                    DeliveryStatus.Preparing => "#3B82F6", // Blue
                    DeliveryStatus.OutForDelivery => "#8B5CF6", // Purple
                    DeliveryStatus.Delivered => "#10B981", // Green
                    _ => "#6B7280" // Gray
                };
            }
            return "#6B7280";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }


    /// <summary>
    /// Converts <see cref="DeliveryStatus"/> enums into user-friendly status strings 
    /// (e.g., "Out for Delivery").
    /// </summary>
    public class OrderStatusToTextConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DeliveryStatus status)
            {
                return status switch
                {
                    DeliveryStatus.Pending => "Order Received",
                    DeliveryStatus.Preparing => "Preparing",
                    DeliveryStatus.OutForDelivery => "Out for Delivery",
                    DeliveryStatus.Delivered => "Delivered",
                    _ => status.ToString()
                };
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }


    /// <summary>
    /// Maps <see cref="DeliveryStatus"/> to specific Segoe Fluent Icon glyph codes 
    /// for visual status indicators.
    /// </summary>
    public class OrderStatusToIconConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DeliveryStatus status)
            {
                return status switch
                {
                    DeliveryStatus.Pending => "&#xE916;", // Clock
                    DeliveryStatus.Preparing => "&#xE7B8;", // Package
                    DeliveryStatus.OutForDelivery => "&#xE7C4;", // Truck
                    DeliveryStatus.Delivered => "&#xE73E;", // CheckCircle
                    _ => "&#xE916;"
                };
            }
            return "&#xE916;";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Returns <see cref="Visibility.Visible"/> only when the status is 'Delivered'. 
    /// Useful for showing "Rate Order" or "Review" buttons.
    /// </summary>

    public class OrderStatusToVisibilityConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DeliveryStatus status)
            {
                // Show review button only for delivered orders
                return status == DeliveryStatus.Delivered ?
                    Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }


    /// <summary>
    /// Returns a boolean indicating if an order is eligible for reordering 
    /// (true only if the status is 'Delivered').
    /// </summary>
    public class OrderStatusToReorderConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DeliveryStatus status)
            {
                // Enable reorder only for delivered orders
                return status == DeliveryStatus.Delivered;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

}
