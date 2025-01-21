namespace TaggedImageViewer.Utils;

public interface IConfigService
{
    T? GetConfig<T>(string key);
    T GetConfig<T>(string key, T defaultValue);
    void SetConfig<T>(string key, T value);
}