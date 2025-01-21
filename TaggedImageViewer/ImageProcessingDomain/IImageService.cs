using System.Windows.Media.Imaging;

namespace TaggedImageViewer.ImageProcessingDomain;

public interface IImageService
{
    BitmapImage LoadImage(string? path, int width, int height);
}