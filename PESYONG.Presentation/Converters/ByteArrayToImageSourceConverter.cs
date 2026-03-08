using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using Windows.Storage.Streams;

namespace PESYONG.Presentation.Converters;

public class ByteArrayToImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        try
        {
            if (value is not byte[] bytes || bytes.Length == 0)
                return null;

            var image = new BitmapImage();

            using var stream = new MemoryStream(bytes);
            using var randomAccessStream = new InMemoryRandomAccessStream();

            stream.CopyTo(randomAccessStream.AsStreamForWrite());
            randomAccessStream.Seek(0);

            image.SetSource(randomAccessStream);
            return image;
        }
        catch
        {
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}