using System.Collections.ObjectModel;
using PropertyChanged;

namespace TaggedImageViewer.ViewModels;

[AddINotifyPropertyChangedInterface]
public class MainWindowViewModel
{
    public ICollection<DirectoryItem> Collections { get; set;  } = [];
    public ObservableCollection<FileItem> Drawings { get; set; } = [];
    public FileItem? SelectedDrawing { get; set;  } = null;

    public double Progress { get; set; } = 0;
    public double ProgressMax { get; set; } = 1;
}