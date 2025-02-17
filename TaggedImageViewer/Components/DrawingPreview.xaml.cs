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
    private DrawingPreviewViewModel _viewModel = null!;
    
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
    
    private delegate void TransformAction(ref Matrix transform);
    
    private void ApplyTransform(TransformAction action)
    {
        Matrix transform = ImagesPanel.RenderTransform.Value;
        action(ref transform);
        ImagesPanel.RenderTransform = new MatrixTransform(transform);
    }
    
    private void OnImageZoom(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta == 0)
            return;

        double scale = e.Delta > 0 
            ? 1.1 
            : 1 / 1.1;
        
        var pos = e.GetPosition(RootPanel);
        ApplyTransform((ref Matrix t) => t.ScaleAt(scale, scale, pos.X, pos.Y));
    }

    private void OnImageMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Panel panel)
            return;

        double centerX = panel.ActualWidth / 2;
        double centerY = panel.ActualHeight / 2;
        
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
            ApplyTransform((ref Matrix t) => t.ScaleAtPrepend(-1, 1, centerX, centerY));
        }
        else if (e.ChangedButton == MouseButton.XButton2)
        {
            ApplyTransform((ref Matrix t) => t.ScaleAtPrepend(1, -1, centerX, centerY));
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
        
        Point mousePos = e.GetPosition(RootPanel);
        Vector displacement = mousePos - _previousMousePosition;
        ApplyTransform((ref Matrix t) => t.Translate(displacement.X, displacement.Y));
        
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