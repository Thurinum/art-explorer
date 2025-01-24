using Microsoft.Win32;

namespace TaggedImageViewer.Utils;

public class ConfigService : IConfigService
{
    private const string SubKeyName = @"Software\Thurinum\TaggedImageViewer";

    public T? GetConfig<T>(string key)
    {
        object? value = GetRegistryKey(SubKeyName).GetValue(key);
        return value == null ? default : (T)value;
    }

    public T GetConfig<T>(string key, T defaultValue)
    {
        var value = GetConfig<T>(key);
        return value == null ? defaultValue : value;
    }

    public void SetConfig<T>(string key, T value)
    {
        var registryKey = GetRegistryKey(SubKeyName);
        
        if (value != null)
            registryKey.SetValue(key, value);
        
        registryKey.Close();
    }

    private static RegistryKey GetRegistryKey(string key)
    {
        return Registry.CurrentUser.OpenSubKey(key, true) 
            ?? Registry.CurrentUser.CreateSubKey(key);
    }
}