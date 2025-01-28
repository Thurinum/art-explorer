using System.Windows;
using PropertyChanged;

namespace TaggedImageViewer.ViewModels;

[AddINotifyPropertyChangedInterface]
public class DrawingPreviewViewModel
{
    public FileItem? SelectedDrawing { get; set;  } = null;
    public FileItem? SelectedDrawing2 { get; set;  } = null;
    public bool IsSecondImageVisible { get; set; } = false;
    
    // used by binding
    public Visibility ShowSecondImage => SelectedDrawing2 != null && IsSecondImageVisible
        ? Visibility.Visible
        : Visibility.Collapsed;
}