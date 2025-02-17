using System.Windows;
using PropertyChanged;

namespace TaggedImageViewer.ViewModels;

[AddINotifyPropertyChangedInterface]
public class DrawingPreviewViewModel
{
    public ICollection<FileItem> SelectedDrawings { get; set; } = [];
    public int SelectedDrawingIndex { get; set; }
    
    public FileItem? SelectedDrawing => SelectedDrawings.ElementAtOrDefault(SelectedDrawingIndex);
    public string PagingInfo => $" ({SelectedDrawingIndex + 1}/{SelectedDrawings.Count})";
}