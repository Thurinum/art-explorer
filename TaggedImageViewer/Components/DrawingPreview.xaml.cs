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
    private DrawingPreviewViewModel _viewModel = null!; // lateinit
    
    public DrawingPreview()
    {
        InitializeComponent();
    }
    
    private void OnComponentLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel = (DrawingPreviewViewModel)DataContext;
    }
    
    public void ResetZoom()
    {
        ImagesPanel.RenderTransform = new ScaleTransform(1, 1);
    }
    
    private void OnImageZoom(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta == 0)
            return;

        Matrix transform = ImagesPanel.RenderTransform.Value;
        double scale = e.Delta > 0 
            ? 1.1 
            : 1 / 1.1;
        
        var pos = e.GetPosition(RootPanel);
        transform.ScaleAt(scale, scale, pos.X, pos.Y);
        ImagesPanel.RenderTransform = new MatrixTransform(transform);
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
        
        Matrix transform = ImagesPanel.RenderTransform.Value;
        Point mousePos = e.GetPosition(RootPanel);
        Vector displacement = mousePos - _previousMousePosition;
        transform.Translate(displacement.X, displacement.Y);
        ImagesPanel.RenderTransform = new MatrixTransform(transform);
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