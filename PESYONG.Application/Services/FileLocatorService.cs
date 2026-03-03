using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;

namespace PESYONG.ApplicationLogic.Services;

public class FileLocatorService
{
    private readonly string _baseStoragePath;

    public FileLocatorService()
    {
        _baseStoragePath = ApplicationData.Current.LocalFolder.Path;
    }

    public async Task<string> SaveImageAsync(StorageFile file, string subFolder = "products")
    {
        // Create subfolder if needed
        var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
            subFolder, CreationCollisionOption.OpenIfExists);

        // Generate unique filename
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.Name)}";
        var storageFile = await file.CopyAsync(folder, fileName, NameCollisionOption.ReplaceExisting);

        // Return the relative path for database storage
        return Path.Combine(subFolder, fileName).Replace("\\", "/");
    }

    public async Task<BitmapImage> LoadImageAsync(string storageLink)
    {
        if (string.IsNullOrEmpty(storageLink))
            return null;

        try
        {
            var fullPath = Path.Combine(_baseStoragePath, storageLink);
            var file = await StorageFile.GetFileFromPathAsync(fullPath);

            using var stream = await file.OpenReadAsync();
            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(stream);

            return bitmapImage;
        }
        catch (FileNotFoundException)
        {
            return null; // File doesn't exist
        }
    }

    public async Task<bool> DeleteImageAsync(string storageLink)
    {
        if (string.IsNullOrEmpty(storageLink))
            return false;

        try
        {
            var fullPath = Path.Combine(_baseStoragePath, storageLink);
            var file = await StorageFile.GetFileFromPathAsync(fullPath);
            await file.DeleteAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool ImageExists(string storageLink)
    {
        if (string.IsNullOrEmpty(storageLink))
            return false;

        var fullPath = Path.Combine(_baseStoragePath, storageLink);
        return File.Exists(fullPath);
    }
}
