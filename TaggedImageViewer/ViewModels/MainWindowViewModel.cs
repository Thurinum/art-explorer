using System.Collections.ObjectModel;
using System.Windows;
using PropertyChanged;

namespace TaggedImageViewer.ViewModels;

[AddINotifyPropertyChangedInterface]
public class MainWindowViewModel
{
    public ICollection<DirectoryItem> Collections { get; set;  } = [];
    public ObservableCollection<FileItem> Drawings { get; set; } = [];
    public FileItem? SelectedDrawing { get; set;  } = null;
    public FileItem? SelectedDrawing2 { get; set;  } = null;
    public bool IsSecondImageVisible { get; set; } = false;
    public string RootDirectory { get; set; } = "";
    
    // used by binding
    public Visibility ShowSecondImage => SelectedDrawing2 != null && IsSecondImageVisible
        ? Visibility.Visible
        : Visibility.Collapsed;

    public double Progress { get; set; } = 0;
    public double ProgressMax { get; set; } = 1;
}