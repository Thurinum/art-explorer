using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using MimeMapping;
using TaggedImageViewer.ImageProcessingDomain;

namespace TaggedImageViewer.FileSystemDomain;

public class DrawingsDirectoryService(IImageService imageService) : IDirectoryService
{
    public List<DirectoryItem> GetRelevantDirectories(string rootPath)
    {
        return GetImageDirectories(rootPath);
    }

    private List<DirectoryItem> GetImageDirectories(string rootPath)
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
            
            if (!imageFiles.Any())
                continue;
            
            var thumbnail = imageService.LoadImage(imageFiles.FirstOrDefault(), 100, 0);
            directories.Add(new DirectoryItem
            {
                DisplayName = Path.GetFileName(directory),
                FullPath = directory,
                Thumbnail = thumbnail,
                GimpFiles = gimpFiles,
                ImageFiles = imageFiles
            });
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