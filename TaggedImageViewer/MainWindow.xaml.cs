using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MessagePack;
using TaggedImageViewer.FileSystemDomain;
using TaggedImageViewer.ImageProcessingDomain;
using TaggedImageViewer.Utils;
using TaggedImageViewer.ViewModels;
using Path = System.IO.Path;
using Vector = System.Windows.Vector;

namespace TaggedImageViewer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly IDirectoryService _directoryService;
    private readonly IImageService _imageService;
    private readonly MainWindowViewModel _viewModel = new();
    private bool _isDraggingImage;
    private Point _previousMousePosition;
    private CancellationTokenSource? _asyncImageLoadCancellation;
    private Dictionary<string, byte[]> _cachedThumbnails = new();
    
    public MainWindow(IDirectoryService directoryService, IImageService imageService)
    {
        _directoryService = directoryService;
        _imageService = imageService;
        InitializeComponent();
        DeserializeThumbnailsCache();
        
        DataContext = _viewModel;
        Refresh();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        _asyncImageLoadCancellation?.Cancel();
        
        SerializeThumbnailsCache();
        base.OnClosing(e);
    }

    private void SerializeThumbnailsCache()
    {
        try
        {
            byte[] data = MessagePackSerializer.Serialize(_cachedThumbnails);
            File.WriteAllBytes("thumbnails.cache", data);
        } 
        catch (Exception e)
        {
            MessageBox.Show($"Failed to save thumbnail cache: {e.Message}");
        }
    }
    
    private void DeserializeThumbnailsCache()
    {
        try
        {
            byte[] data = File.ReadAllBytes("thumbnails.cache");
            _cachedThumbnails = MessagePackSerializer.Deserialize<Dictionary<string, byte[]>>(data);
            
            // remove invalid entries
            foreach (var key in _cachedThumbnails.Keys.ToArray())
            {
                if (!File.Exists(key))
                    _cachedThumbnails.Remove(key);
            }
        } 
        catch (Exception e)
        {
            MessageBox.Show($"Failed to load thumbnail cache: {e.Message}");
        }
    }

    private void Refresh()
    {
        // todo: get from config and configure somewhere in app
        _viewModel.Collections = _directoryService.GetRelevantDirectories("F:\\Graphics\\Drawings");
    }

    private void OnSelectDirectory(object sender, SelectionChangedEventArgs e)
    {
        if (DirectoryListBox.SelectedItem is not DirectoryItem selectedDir)
            return;

        _viewModel.Progress = 0;
        _viewModel.ProgressMax = selectedDir.GimpFiles.Count() + selectedDir.ImageFiles.Count();
        _viewModel.Drawings.Clear();
        
        // todo: merge file query into one in the service
        foreach (string imagePath in selectedDir.ImageFiles)
        {
            _viewModel.Drawings.Add(new FileItem(
                DisplayName: Path.GetFileName(imagePath), 
                FullPath: imagePath,
                Type: FileItemType.ImageFile, 
                Thumbnail: null!,
                IsLoadingThumbnail: true
            ));
        }
        foreach (string xcfPath in selectedDir.GimpFiles)
        {
            _viewModel.Drawings.Add(new FileItem(
                DisplayName: Path.GetFileNameWithoutExtension(xcfPath), 
                FullPath: xcfPath,
                Type: FileItemType.XcfFile, 
                Thumbnail: null!,
                IsLoadingThumbnail: true
            ));
        }
        _asyncImageLoadCancellation?.Cancel();
        _asyncImageLoadCancellation = new CancellationTokenSource();
        var cancellationToken = _asyncImageLoadCancellation.Token;
        
        Task.Run(() =>
        {
            Stopwatch sw = new();
            sw.Start();
            
            IEnumerable<string> allFiles = selectedDir.ImageFiles.Concat(selectedDir.GimpFiles).ToArray();
            
            for (int i = 0; i < allFiles.Count(); i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                
                var filePath = allFiles.ElementAt(i);

                BitmapImage thumbnail = _imageService.GetDefaultThumbnail();
                if (_cachedThumbnails.TryGetValue(filePath, out var cachedThumbnail))
                {
                    thumbnail = BitmapImageExtensions.FromByteArray(cachedThumbnail);
                }
                else
                {
                    var bitmap = _imageService.LoadImage(filePath, 1024, 0);
                    if (bitmap.IsOk())
                    {
                        thumbnail = bitmap.Result();
                        _cachedThumbnails[filePath] = thumbnail.ToByteArray();
                    }
                }

                var index = i;
                Dispatcher.InvokeAsync(() =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    FileItem drawing = _viewModel.Drawings[index];
                    _viewModel.Drawings[index] = drawing with
                    {
                        Thumbnail = thumbnail,
                        IsLoadingThumbnail = false
                    };
                    
                    _viewModel.Progress++;
                });
            }
            
            Console.WriteLine($"Loaded thumbnails in {sw.ElapsedMilliseconds}ms");
            sw.Stop();
        }, cancellationToken);
    }
    
    private void OnOpenWithAssociatedApp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListBox listBox)
            return;

        if (listBox.SelectedItem is not FileItem selectedFile)
            return;
        
        Process.Start(new ProcessStartInfo
        {
            FileName = selectedFile.FullPath, 
            UseShellExecute = true, 
            Verb = "open"
        });
    }
    
    private void OnImageZoom(object sender, MouseWheelEventArgs e)
    {
        // zoom in/out at mouse position
        if (sender is not Panel panel)
            return;
        
        Image? image = panel.Children.OfType<Image>().FirstOrDefault();
        if (image == null)
            return;

        if (e.Delta == 0)
            return;

        Matrix transform = image.RenderTransform.Value;
        double scale = e.Delta > 0 
            ? 1.1 
            : 1 / 1.1;
        
        var pos = e.GetPosition(panel);
        transform.ScaleAt(scale, scale, pos.X, pos.Y);
        image.RenderTransform = new MatrixTransform(transform);
    }

    private void OnImageDragStart(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Panel panel)
            return;
        
        _isDraggingImage = true;
        _previousMousePosition = e.GetPosition(panel);
    }

    private void OnImageDragEnd(object sender, MouseButtonEventArgs e)
    {
        _isDraggingImage = false;
    }

    private void OnImageDrag(object sender, MouseEventArgs e)
    {
        if (!_isDraggingImage)
            return;
        
        if (sender is not Panel panel)
            return;
        
        Image? image = panel.Children.OfType<Image>().FirstOrDefault();
        if (image == null)
            return;
        
        Matrix transform = image.RenderTransform.Value;
        Point mousePos = e.GetPosition(panel);
        Vector displacement = mousePos - _previousMousePosition;
        transform.Translate(displacement.X, displacement.Y);
        image.RenderTransform = new MatrixTransform(transform);
        _previousMousePosition = mousePos;
    }

    private void OnShowDrawingDetails(object sender, MouseButtonEventArgs e)
    {
        MainLayout.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
        MainLayout.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Pixel);
        MainLayout.ColumnDefinitions[3].Width = new GridLength(0, GridUnitType.Pixel);
    }

    private void OnSelectFile(object sender, RoutedEventArgs e)
    {
        if (sender is not ListBox listBox)
            return;
        
        if (listBox.SelectedItem is not FileItem selectedFile)
            return;
        
        _viewModel.SelectedDrawing = selectedFile;
        ImageViewer.RenderTransform = new ScaleTransform(1, 1);

        if (listBox.SelectedItems.Count == 1)
        {
            _viewModel.SelectedDrawing2 = null;
            return;
        }

        if (listBox.SelectedItems[1] is not FileItem selectedFile2) 
            return;
        
        _viewModel.SelectedDrawing2 = selectedFile2;
    }

    private void DeleteThumbnailsCache(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show("Are you sure?",
            "Delete Thumbnails Cache?", MessageBoxButton.YesNo);
        
        if (result != MessageBoxResult.Yes) 
            return;
        
        _cachedThumbnails.Clear();
        SerializeThumbnailsCache();
    }
    
    private void OnEnableCompare(object sender, MouseButtonEventArgs e)
    {
        _viewModel.IsSecondImageVisible = true;
    }

    private void OnDisableCompare(object sender, MouseButtonEventArgs e)
    {
        _viewModel.IsSecondImageVisible = false;
    }
}