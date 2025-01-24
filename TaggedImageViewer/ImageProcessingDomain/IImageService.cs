using System.Windows.Media.Imaging;
using TaggedImageViewer.Utils;

namespace TaggedImageViewer.ImageProcessingDomain;

public interface IImageService
{
    BitmapImage GetDefaultImage();
    OneOf<BitmapImage, FuckYou> LoadImage(string? path, int width, int height);
}