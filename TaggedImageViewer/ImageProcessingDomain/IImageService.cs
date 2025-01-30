using System.Windows.Media.Imaging;
using TaggedImageViewer.Utils;

namespace TaggedImageViewer.ImageProcessingDomain;

public interface IImageService
{
    BitmapImage GetDefaultThumbnail();
    OneOf<BitmapImage, FuckYou> LoadImage(string? path, uint width, uint height);
}