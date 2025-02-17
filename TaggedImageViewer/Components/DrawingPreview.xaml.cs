using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TaggedImageViewer.ViewModels;

namespace TaggedImageViewer.Components;

public partial class DrawingPreview
{
    private DrawingPreviewViewModel _viewModel = null!;
    private bool _isDraggingImage;
    private Point _previousMousePosition;

    public DrawingPreview()
    {
        InitializeComponent();
    }

    private void OnComponentLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel = (DrawingPreviewViewModel)DataContext;
    }
    
    public void ResetTransform()
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

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
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
            ResetTransform();
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

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        _isDraggingImage = false;
    }
    
    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta == 0)
            return;
        
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            SwitchImage(e.Delta < 0 ? 1 : -1);
            return;
        }

        double scale = e.Delta > 0 
            ? 1.1 
            : 1 / 1.1;
        
        var pos = e.GetPosition(RootPanel);
        ApplyTransform((ref Matrix t) => t.ScaleAt(scale, scale, pos.X, pos.Y));
    }
    
    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDraggingImage)
            return;
        
        Point mousePos = e.GetPosition(RootPanel);
        Vector displacement = mousePos - _previousMousePosition;
        ApplyTransform((ref Matrix t) => t.Translate(displacement.X, displacement.Y));
        
        _previousMousePosition = mousePos;
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        _isDraggingImage = false;
    }

    private void SwitchImage(int offset)
    {
        _viewModel.SelectedDrawingIndex = (_viewModel.SelectedDrawingIndex + offset) % _viewModel.SelectedDrawings.Count;
        
        if (_viewModel.SelectedDrawingIndex < 0)
            _viewModel.SelectedDrawingIndex = _viewModel.SelectedDrawings.Count - 1;
    }
}