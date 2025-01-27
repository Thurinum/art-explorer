using System.Windows;

namespace TaggedImageViewer.Components;

public class PickedDirectoryChangedEventArgs(RoutedEvent routedEvent, string newDirectory)
    : RoutedEventArgs(routedEvent)
{
    public string PickedDirectory { get; } = newDirectory;
}