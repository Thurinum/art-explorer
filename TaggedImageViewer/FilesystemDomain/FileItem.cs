using System.Windows.Media.Imaging;

namespace TaggedImageViewer;

public enum FileItemType
{
    XcfFile,
    ImageFile
}

public record DirectoryItem
(
    string DisplayName,
    string FullPath,
    BitmapImage Thumbnail,
    IEnumerable<string> GimpFiles,
    IEnumerable<string> ImageFiles
);

public record FileItem
(
    string DisplayName,
    string FullPath,
    FileItemType Type,
    BitmapImage Thumbnail,
    bool IsLoadingThumbnail = false
);
