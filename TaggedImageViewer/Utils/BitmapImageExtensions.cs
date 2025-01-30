using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageMagick;
using ImageMagick.ImageOptimizers;

namespace TaggedImageViewer.Utils;

public static class BitmapImageExtensions
{
    // todo: to service
    public static BitmapImage FromByteArray(byte[] data)
    {
        using MemoryStream stream = new(data);
        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = stream;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();
        bitmapImage.Freeze();

        return bitmapImage;
    }

    public static byte[] ToByteArray(this BitmapImage image)
    {
        using MemoryStream stream = new();
        BitmapEncoder encoder = new JpegBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));
        encoder.Save(stream);

        return stream.ToArray();
    }
}