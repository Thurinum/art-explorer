using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TaggedImageViewer.FileSystemDomain;
using TaggedImageViewer.ImageProcessingDomain;
using Path = System.IO.Path;

namespace TaggedImageViewer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly IDirectoryService _directoryService;
    private readonly IImageService _imageService;
    
    public MainWindow(IDirectoryService directoryService, IImageService imageService)
    {
        _directoryService = directoryService;
        _imageService = imageService;
        InitializeComponent();
       
        IEnumerable<DirectoryItem> directories = _directoryService.GetRelevantDirectories("F:\\Graphics\\Drawings");
        DirectoryListBox.ItemsSource = directories;
    }

    private void DirectoryListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DirectoryListBox.SelectedItem is not DirectoryItem selectedDir)
            return;

        List<DirectoryItem> d = [];
        foreach (string gimpFile in selectedDir.GimpFiles)
        {
            string name = Path.GetFileName(gimpFile);
            BitmapImage bitmap = _imageService.LoadImage(gimpFile, 100, 0);
            
            d.Add(new DirectoryItem
            {
                DisplayName = name,
                FullPath = gimpFile,
                Thumbnail = bitmap
            });
        }
        
        List<DirectoryItem> d2 = [];
        foreach (string imageFile in selectedDir.ImageFiles)
        {
            string name = Path.GetFileName(imageFile);
            BitmapImage bitmap = _imageService.LoadImage(imageFile, 100, 0);
            
            d2.Add(new DirectoryItem
            {
                DisplayName = name,
                FullPath = imageFile,
                Thumbnail = bitmap
            });
        }
        
        XcfFilesListBox.ItemsSource = d;
        ImgListBox.ItemsSource = d2;
        
        // Process.Start(new ProcessStartInfo
        // {
        //     FileName = selectedDir.FullPath,
        //     UseShellExecute = true,
        //     Verb = "open"
        // });
    }
}