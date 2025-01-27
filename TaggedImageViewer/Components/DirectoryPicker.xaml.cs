using System.Windows;
using Microsoft.Win32;

namespace TaggedImageViewer.Components;

public partial class DirectoryPicker
{
    public DirectoryPicker()
    {
        InitializeComponent();
    }
    
    public static readonly DependencyProperty PickedDirectoryProperty = DependencyProperty.Register(
        nameof(PickedDirectory), typeof(string), typeof(DirectoryPicker), new PropertyMetadata(default(string)));
    
    public string PickedDirectory
    {
        get => (string)GetValue(PickedDirectoryProperty);
        set => SetValue(PickedDirectoryProperty, value);
    }
    
    public static readonly RoutedEvent PickedDirectoryChangedEvent =
        EventManager.RegisterRoutedEvent("PickedDirectoryChanged", RoutingStrategy.Bubble, typeof(EventHandler<PickedDirectoryChangedEventArgs>), typeof(DirectoryPicker));

    public event EventHandler<PickedDirectoryChangedEventArgs> PickedDirectoryChanged
    {
        add => AddHandler(PickedDirectoryChangedEvent, value);
        remove => RemoveHandler(PickedDirectoryChangedEvent, value);
    }

    private void OnPickDirectory(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFolderDialog();
        if (openFileDialog.ShowDialog() != true) 
            return;

        PickedDirectory = openFileDialog.FolderName;
        RaiseEvent(new PickedDirectoryChangedEventArgs(PickedDirectoryChangedEvent, PickedDirectory));
    }
}