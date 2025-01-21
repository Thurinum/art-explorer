using System.Windows.Media.Imaging;

namespace TaggedImageViewer.FileSystemDomain;

public interface IDirectoryService
{
    List<DirectoryItem> GetRelevantDirectories(string rootPath);
}