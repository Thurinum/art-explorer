using System.Windows.Media.Imaging;

namespace TaggedImageViewer;

public struct DirectoryItem
{
    public string DisplayName { get; set; }
    public string FullPath { get; set; }
    public BitmapImage Thumbnail { get; set; } 
    public IEnumerable<string> GimpFiles { get; set; }
    public IEnumerable<string> ImageFiles { get; set; }
}
