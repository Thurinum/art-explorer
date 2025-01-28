using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TaggedImageViewer.ViewModels;

namespace TaggedImageViewer.Components;

public partial class DrawingPreview : UserControl
{
    private bool _isDraggingImage;
    private Point _previousMousePosition;
    private readonly DrawingPreviewViewModel _viewModel; // todo: use a dedicated view model
    
    public DrawingPreview()
    {
        InitializeComponent();
        _viewModel = (DrawingPreviewViewModel)DataContext;
    }
    
    public void ResetZoom()
    {
        ImageViewer.RenderTransform = new ScaleTransform(1, 1);
    }
    
    private void OnImageZoom(object sender, MouseWheelEventArgs e)
    {
        // zoom in/out at mouse position
        if (sender is not Panel panel)
            return;
        
        Panel? viewer = panel.Children.OfType<Panel>().FirstOrDefault();
        if (viewer == null)
            return;

        if (e.Delta == 0)
            return;

        Matrix transform = viewer.RenderTransform.Value;
        double scale = e.Delta > 0 
            ? 1.1 
            : 1 / 1.1;
        
        var pos = e.GetPosition(panel);
        transform.ScaleAt(scale, scale, pos.X, pos.Y);
        viewer.RenderTransform = new MatrixTransform(transform);
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
        
        Panel? viewer = panel.Children.OfType<Panel>().FirstOrDefault();
        if (viewer == null)
            return;
        
        Matrix transform = viewer.RenderTransform.Value;
        Point mousePos = e.GetPosition(panel);
        Vector displacement = mousePos - _previousMousePosition;
        transform.Translate(displacement.X, displacement.Y);
        viewer.RenderTransform = new MatrixTransform(transform);
        _previousMousePosition = mousePos;
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