using System.IO;
using System.Windows.Media.Imaging;
using ImageMagick;

namespace TaggedImageViewer.ImageProcessingDomain;

/// <summary>
/// Has custom support for XCF files.
/// </summary>
public class ImageProcessingService() : IImageService
{
    private const string DefaultImagePath = "pack://application:,,,/Assets/invalid_thumbnail.png";
    
    public BitmapImage LoadImage(string? path, int width, int height)
    {
        if (path == null)
            return new BitmapImage(new Uri(DefaultImagePath));
        
        return path.EndsWith(".xcf") 
            ? LoadXcfImage(path, (uint)width, (uint)height) 
            : new BitmapImage(new Uri(DefaultImagePath));
    }
    
    private BitmapImage LoadGenericImage(string path, int width, int height)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(path);
        bitmap.DecodePixelWidth = width;
        bitmap.DecodePixelHeight = height;
        bitmap.EndInit();
        return bitmap;
    }
    
    private BitmapImage LoadXcfImage(string path, uint width, uint height)
    {
        byte[] imageData = File.ReadAllBytes("input.xcf");
        using (MemoryStream ms = new MemoryStream(imageData))
        {
            using (MagickImage image = new MagickImage(ms))
            {
                // Process your image
            }
        }

        
        try
        {
            MagickReadSettings settings = new MagickReadSettings
            {
                Format = MagickFormat.Xcf,
                Width = width,
                Height = height,
                Depth = 8
            };

            using MagickImageCollection layers = new MagickImageCollection(path, settings);
            using IMagickImage<byte> image = layers.First();
            
            // image.Alpha(AlphaOption.On);
            // image.Resize(width, height);

            using var memoryStream = new MemoryStream();
            image.Write(memoryStream, MagickFormat.Bmp);
            byte[] data = memoryStream.ToArray();
        
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = new MemoryStream(data);
            bitmap.EndInit();
            bitmap.Freeze();
        
            return bitmap;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new BitmapImage(new Uri(DefaultImagePath));   
        }
    }
}