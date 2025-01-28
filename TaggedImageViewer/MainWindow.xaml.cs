using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MessagePack;
using TaggedImageViewer.Components;
using TaggedImageViewer.FileSystemDomain;
using TaggedImageViewer.ImageProcessingDomain;
using TaggedImageViewer.Utils;
using TaggedImageViewer.ViewModels;
using Path = System.IO.Path;

namespace TaggedImageViewer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly IDirectoryService _directoryService;
    private readonly IImageService _imageService;
    private readonly IConfigService _configService;
    private readonly MainWindowViewModel _viewModel = new();
    private readonly FileSystemWatcher _watcher = new();

    private CancellationTokenSource? _thumbnailLoadCancel;
    private Dictionary<string, byte[]> _cachedThumbnails = new();

    public MainWindow(IDirectoryService directoryService, IImageService imageService, IConfigService configService)
    {
        _directoryService = directoryService;
        _imageService = imageService;
        _configService = configService;
        InitializeComponent();
        DeserializeThumbnailsCache();

        _viewModel.RootDirectory = _configService.GetConfig<string>("RootDirectory", "");

        _watcher.Path = _viewModel.RootDirectory;
        _watcher.NotifyFilter = NotifyFilters.LastWrite 
                              | NotifyFilters.FileName 
                              | NotifyFilters.DirectoryName;
        _watcher.Filter = "*.*";
        //_watcher.Created += OnDrawingRenamed;
        _watcher.Renamed += OnDrawingRenamed;
        //_watcher.Changed += OnDrawingModified;
        //_watcher.Deleted += OnDrawingRenamed;
        _watcher.IncludeSubdirectories = true;
        _watcher.EnableRaisingEvents = true;
        
        DataContext = _viewModel;
        Refresh();
    }

    private void OnDrawingRenamed(object sender, RenamedEventArgs e)
    {
        int idx = _viewModel.Drawings.ToList().FindIndex(f => f.FullPath == e.OldFullPath);
        if (idx == -1)
            return;
        
        Dispatcher.InvokeAsync(() =>
        {
            _viewModel.Drawings[idx] = _viewModel.Drawings[idx] with
            {
                FullPath = e.FullPath,
                DisplayName = Path.GetFileName(e.FullPath)
            };
        });
    }

    private void OnDrawingModified(object sender, FileSystemEventArgs e)
    {
        int idx = _viewModel.Drawings.ToList().FindIndex(f => f.FullPath == e.FullPath);
        if (idx == -1)
            return;
        
        var item = _viewModel.Drawings[idx];

        LoadThumbnailsAsync([ item.FullPath ]);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        _thumbnailLoadCancel?.Cancel();
        
        _configService.SetConfig("RootDirectory", _viewModel.RootDirectory);
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
        _viewModel.Collections = _directoryService.GetRelevantDirectories(_viewModel.RootDirectory);
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
        
        string[] allFiles = selectedDir.ImageFiles.Concat(selectedDir.GimpFiles).ToArray();
        LoadThumbnailsAsync(allFiles);
    }

    private void LoadThumbnailsAsync(string[] items)
    {
        _thumbnailLoadCancel?.Cancel();
        _thumbnailLoadCancel = new CancellationTokenSource();
         
        Task.Run(() =>
        {
            Stopwatch sw = new();
            sw.Start();
            
            foreach (var item in items)
            {
                // todo: slow and stupid
                var index = _viewModel.Drawings.ToList().FindIndex(f => f.FullPath == item);
                LoadThumbnail(index, item, _thumbnailLoadCancel.Token);
            }
            
            Console.WriteLine($"Loaded thumbnails in {sw.ElapsedMilliseconds}ms");
            sw.Stop();
        }, _thumbnailLoadCancel.Token);
    }

    // todo: move to service
    private void LoadThumbnail(int index, string filePath, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        
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

    private void OnSelectFile(object sender, RoutedEventArgs e)
    {
        if (sender is not ListBox listBox)
            return;
        
        if (listBox.SelectedItem is not FileItem selectedFile)
            return;
        
        _viewModel.DrawingPreview.SelectedDrawing = selectedFile;
        PreviewComponent.ResetZoom();

        if (listBox.SelectedItems.Count == 1)
        {
            _viewModel.DrawingPreview.SelectedDrawing2 = null;
            return;
        }

        if (listBox.SelectedItems[1] is not FileItem selectedFile2) 
            return;
        
        _viewModel.DrawingPreview.SelectedDrawing2 = selectedFile2;
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
    
    private void OnPickRootDirectory(object sender, PickedDirectoryChangedEventArgs e)
    {
        _viewModel.RootDirectory = e.PickedDirectory;
        Refresh();
    }
}