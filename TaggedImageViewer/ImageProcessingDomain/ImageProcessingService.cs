using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using ImageMagick;
using TaggedImageViewer.Utils;

namespace TaggedImageViewer.ImageProcessingDomain;

/// <summary>
/// Has custom support for XCF files.
/// </summary>
public class ImageProcessingService() : IImageService
{
    private const string DefaultImagePath = "pack://application:,,,/Assets/invalid_thumbnail.png";
    private static readonly BitmapImage DefaultImage = new(new Uri(DefaultImagePath));

    public BitmapImage GetDefaultThumbnail()
    {
        return DefaultImage;
    }

    public OneOf<BitmapImage, FuckYou> LoadImage(string? path, uint width, uint height)
    {
        if (path == null)
            return DefaultImage;
        
        return path.EndsWith(".xcf") 
            ? LoadXcfImage(path, width, height) 
            : LoadGenericImage(path, width, height);
    }

    private static OneOf<BitmapImage, FuckYou> LoadGenericImage(string path, uint width, uint height)
    {
        try
        {
            using IMagickImage<byte> image = new MagickImage(path);
            image.ColorAlpha(MagickColors.White);
            image.Resize(width, height);
            using MemoryStream stream = new();
            image.Write(stream, MagickFormat.Jpeg);
            
            stream.Position = 0;
            
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            
            return bitmap;
        }
        catch (Exception e)
        {
            return new FuckYou($"Failed to load image at {path} because {e}");
        }
    }
    
    private static OneOf<BitmapImage, FuckYou> LoadXcfImage(string path, uint width, uint height)
    {
        try
        {
            var settings = new MagickReadSettings
            {
                Format = MagickFormat.Xcf,
                Width = width,
                Height = height
            };
            using MagickImageCollection layers = new MagickImageCollection(path, settings);
            using IMagickImage<byte> image = layers.Merge();
            image.BackgroundColor = MagickColors.White;
            using MemoryStream stream = new();
            image.Resize(width, height);
            image.Write(stream, MagickFormat.Jpeg);

            stream.Position = 0;
            
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
        
            return bitmap;
        }
        catch (Exception e)
        {
            return new FuckYou($"Failed to load XCF image at {path} because {e}");
        }
    }
}