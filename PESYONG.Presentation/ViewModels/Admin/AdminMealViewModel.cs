using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace PESYONG.Presentation.ViewModels.Admin;

public class MealViewModel : ObservableObject
{
    public int MealID { get; set; }
    public string MealName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MealPrice { get; set; }
    public string FormattedPrice => MealPrice.ToString("C");
    public int StockQuantity { get; set; }
    public string StockStatus
    {
        get
        {
            if (StockQuantity == 0) return "Out of Stock";
            if (StockQuantity <= 5) return "Low Stock";
            return "In Stock";
        }
    }
    public int MinOrderQuantity { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public string DeliveryType { get; set; } = string.Empty;
    public BitmapImage ImageSource { get; set; }
    public bool IsAvailable => StockQuantity >= MinOrderQuantity;
    public DateTime CreationDate { get; set; }
    public string RelativeCreationTime
    {
        get
        {
            var timeSpan = DateTime.UtcNow - CreationDate;
            if (timeSpan.TotalDays < 1) return "Today";
            if (timeSpan.TotalDays < 7) return $"{timeSpan.Days} days ago";
            if (timeSpan.TotalDays < 30) return $"{timeSpan.Days / 7} weeks ago";
            return $"{timeSpan.Days / 30} months ago";
        }
    }

}
