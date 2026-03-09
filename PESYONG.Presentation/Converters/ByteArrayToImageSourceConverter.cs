using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

namespace PESYONG.Presentation.Converters;

public class ByteArrayToImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        try
        {
            if (value is not byte[] bytes || bytes.Length == 0)
                return null;

            var bitmap = new BitmapImage();

            using var memoryStream = new MemoryStream(bytes);
            bitmap.SetSource(memoryStream.AsRandomAccessStream());

            return bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Image converter error: {ex.Message}");
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}