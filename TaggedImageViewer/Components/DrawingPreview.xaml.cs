using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
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

    private void OnImageMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Panel panel)
            return;

        if (e.ChangedButton == MouseButton.Left)
        {
            _isDraggingImage = true;
            _previousMousePosition = e.GetPosition(panel);
        }
        else if (e.ChangedButton == MouseButton.Middle)
        {
            ResetZoom();
        }
        else if (e.ChangedButton == MouseButton.XButton1)
        {
            // TODO: Find out how to use the current pos as origin for the flip
            Matrix transform = ImagesPanel.RenderTransform.Value;
            transform.ScaleAt(-1, 1, panel.ActualWidth / 2, panel.ActualHeight / 2);
            ImagesPanel.RenderTransform = new MatrixTransform(transform);
        }
        else if (e.ChangedButton == MouseButton.XButton2)
        {
            Matrix transform = ImagesPanel.RenderTransform.Value;
            transform.ScaleAt(1, -1, panel.ActualWidth / 2, panel.ActualHeight / 2);
            ImagesPanel.RenderTransform = new MatrixTransform(transform);
        }
    }

    private void OnImageMouseUp(object sender, MouseButtonEventArgs e)
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