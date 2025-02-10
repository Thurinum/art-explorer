using System.Collections.ObjectModel;
using System.Windows;
using PropertyChanged;

namespace TaggedImageViewer.ViewModels;

[AddINotifyPropertyChangedInterface]
public class MainWindowViewModel
{
    public ICollection<DirectoryItem> Collections { get; set;  } = [];
    public ObservableCollection<FileItem> Drawings { get; set; } = [];
    public DrawingPreviewViewModel DrawingPreview { get; set; } = new();
    public string RootDirectory { get; set; } = "";
    public double Progress { get; set; } = 0;
    public double ProgressMax { get; set; } = 1;
    
    public bool AutoFitZoom { get; set; } = true;
}