using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using MimeMapping;
using TaggedImageViewer.ImageProcessingDomain;
using TaggedImageViewer.Utils;

namespace TaggedImageViewer.FileSystemDomain;

public class DrawingsDirectoryService(IImageService imageService) : IDirectoryService
{
    public List<DirectoryItem> GetRelevantDirectories(string rootPath)
    {
        var directories = new List<DirectoryItem>();
        
        foreach (string directory in Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories))
        {
            // note: we make the assumption that folders containing XCF files won't contain folders that also contain XCF files
            IEnumerable<string> gimpFiles = Directory.GetFiles(directory, ".", SearchOption.TopDirectoryOnly)
                .Where(IsGimpFile)
                .ToList();
            
            if (!gimpFiles.Any())
                continue;

            IEnumerable<string> imageFiles = Directory.GetFiles(directory, ".", SearchOption.AllDirectories)
                .Where(IsImageFile)
                .ToList();
            
            var thumbnail = imageService.LoadImage(imageFiles.FirstOrDefault(), 100, 0);
            if (thumbnail.IsError())
            {
                Debug.WriteLine($"Failed to load thumbnail for {directory} because {thumbnail.Error()}");
                thumbnail = imageService.GetDefaultThumbnail();
                continue;
            }
            directories.Add(new DirectoryItem(
                DisplayName: Path.GetFileName(directory), 
                FullPath: directory,
                Thumbnail: thumbnail.Result(), 
                GimpFiles: gimpFiles, 
                ImageFiles: imageFiles));
        }
        
        return directories;
    }

    private static bool IsImageFile(string filePath)
    {
        var mimeType = MimeUtility.GetMimeMapping(filePath);
        return mimeType.StartsWith("image/");
    }
    
    private static bool IsGimpFile(string filePath)
    {
        // can't use database because XCF files have no registered mime type
        return Path.GetExtension(filePath) == ".xcf";
    }
}