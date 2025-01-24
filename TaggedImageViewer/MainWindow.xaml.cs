using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
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
    private MainWindowViewModel _viewModel = new();
    
    public MainWindow(IDirectoryService directoryService, IImageService imageService)
    {
        _directoryService = directoryService;
        _imageService = imageService;
        InitializeComponent();

        const string registryKey = @"Software\ThurinumDrawingViewer";
        
        var key = Registry.CurrentUser.OpenSubKey(registryKey);
        if (key == null)
        {
            key = Registry.CurrentUser.CreateSubKey(registryKey);
            var username = (string)key.GetValue("RootDirectory", "DefaultUsername");
            key.Close();
        }

        IEnumerable<DirectoryItem> directories = _directoryService.GetRelevantDirectories("F:\\Graphics\\Drawings");
        DirectoryListBox.ItemsSource = directories;
        DataContext = _viewModel;

        Refresh();
    }

    private void Refresh()
    {
        _viewModel.Collections = _directoryService.GetRelevantDirectories("F:\\Graphics\\Drawings");
    }

    private void DirectoryListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DirectoryListBox.SelectedItem is not DirectoryItem selectedDir)
            return;

        _viewModel.Progress = 0;
        _viewModel.ProgressMax = selectedDir.GimpFiles.Count() + selectedDir.ImageFiles.Count();
        _viewModel.XcfDrawings.Clear();
        
        // todo load once only
        BitmapImage tempImage = new BitmapImage(new Uri("pack://application:,,,/Assets/load.gif"));
        
        foreach (string filePath in selectedDir.GimpFiles)
        {
            string name = Path.GetFileName(filePath);
            _viewModel.XcfDrawings.Add(new DirectoryItem
            {
                DisplayName = name,
                FullPath = filePath,
                Thumbnail = tempImage
            });
        }
        
        // todo avoid duplicata
        Task.Run(() =>
        {
            for (int i = 0; i < selectedDir.GimpFiles.Count(); i++)
            {
                string filePath = selectedDir.GimpFiles.ElementAt(i);
                // todo better error handling (hoist up + either monad)
                var bitmap = _imageService.LoadImage(filePath, 100, 0);
                Dispatcher.InvokeAsync(() =>
                {
                    _viewModel.XcfDrawings[i] = new DirectoryItem
                    {
                        DisplayName = Path.GetFileName(filePath),
                        FullPath = filePath,
                        Thumbnail = bitmap.IsOk() 
                            ? bitmap.Result() 
                            // todo cache this
                            : new BitmapImage(new Uri("pack://application:,,,/Assets/invalid_thumbnail.png"))
                    };
                    _viewModel.Progress++;
                });
            }
        });

        _viewModel.ImgDrawings.Clear();
        Task.Run(() =>
        {
            foreach (string filePath in selectedDir.ImageFiles)
            {
                string name = Path.GetFileName(filePath);
                var bitmap = _imageService.LoadImage(filePath, 100, 0);
                
                Dispatcher.InvokeAsync(() => 
                {
                    _viewModel.ImgDrawings.Add(new DirectoryItem
                    {
                        DisplayName = name,
                        FullPath = filePath,
                        Thumbnail = bitmap.IsOk() 
                            ? bitmap.Result() 
                            // todo cache this
                            : new BitmapImage(new Uri("pack://application:,,,/Assets/invalid_thumbnail.png"))
                    });
                    _viewModel.Progress++;
                });
            }
        });
    }
    
    private void FilesListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox listBox)
            return;
        
        if (listBox.SelectedItem is not DirectoryItem selectedFile)
            return;
        
        _viewModel.SelectedDrawing = selectedFile;
    }

    private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListBox listBox)
            return;

        if (listBox.SelectedItem is not DirectoryItem selectedFile)
            return;
        
        Process.Start(new ProcessStartInfo
        {
            FileName = selectedFile.FullPath, 
            UseShellExecute = true, 
            Verb = "open"
        });
    }

    // https://stackoverflow.com/questions/1585462/bubbling-scroll-events-from-a-listview-to-its-parent
    private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled) 
            return;
        
        e.Handled = true;
        var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = MouseWheelEvent,
            Source = sender
        };
        
        var parent = ((Control)sender).Parent as UIElement;
        parent?.RaiseEvent(eventArg);
    }
}