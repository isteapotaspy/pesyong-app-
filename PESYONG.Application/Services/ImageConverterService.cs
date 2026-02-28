using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace PESYONG.ApplicationLogic.Services;

/// <summary>
/// This service ensures seamless transaction between the backend and the frontend image transmission.
/// The backend (ASP.NET & MS SQL) uses byte[] byteArray of data to store images.
/// The frontend (WinUI3) uses BitmapImage to interpret it into .xaml files and display on screen.
/// This service converts ||  byte[] byteArray <-> BitmapImage || from each other, and vice-versa.
/// </summary>

/// <remarks>
/// [Completed, not tested yet] TASK: Implement JpegImageToByteArray task below. 
/// </remarks>

public static class ImageConverterService
{
    public static async Task<BitmapImage> ByteArrayToBitmapImageAsync(byte[] byteArray)
    {
        if (byteArray == null || byteArray.Length == 0)
            return null;

        try
        {
            var bitmapImage = new BitmapImage();
            using (var stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(byteArray.AsBuffer());
                stream.Seek(0);
                await bitmapImage.SetSourceAsync(stream);
            }
            return bitmapImage;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error converting byte array to image: {ex.Message}");
            return null;
        }
    }

    public static async Task<byte[]> SoftwareBitmapToByteArrayAsync(
        SoftwareBitmap softwareBitmap, 
        BitmapEncoderFormat format = BitmapEncoderFormat.Jpeg)
    {
        if (softwareBitmap == null) return null;

        try
        {
            using var stream = new InMemoryRandomAccessStream();

            // Create encoder based on desired format
            BitmapEncoder encoder = null;
            string contentType = "image/jpeg";

            switch (format)
            {
                case BitmapEncoderFormat.Jpeg:
                    encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                    contentType = "image/jpeg";
                    break;
                case BitmapEncoderFormat.Png:
                    encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                    contentType = "image/png";
                    break;
                case BitmapEncoderFormat.Bmp:
                    encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream);
                    contentType = "image/bmp";
                    break;
            }

            if (encoder == null) return null;

            encoder.SetSoftwareBitmap(softwareBitmap);
            await encoder.FlushAsync();

            stream.Seek(0);
            var buffer = new byte[stream.Size];
            await stream.AsStream().ReadAsync(buffer, 0, buffer.Length);

            return buffer;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error converting SoftwareBitmap to byte array: {ex.Message}");
            return null;
        }
    }

    public enum BitmapEncoderFormat
    {
        Jpeg,
        Png,
        Bmp
    }
}