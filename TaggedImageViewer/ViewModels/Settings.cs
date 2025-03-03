using PropertyChanged;

namespace TaggedImageViewer.ViewModels;

[AddINotifyPropertyChangedInterface]
public sealed partial class Settings
{
    public string RootDirectory { get; set; } = "";
    public bool ResetZoomOnImageChange { get; set; } = true;
}