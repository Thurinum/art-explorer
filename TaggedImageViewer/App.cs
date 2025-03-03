using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using TaggedImageViewer.FileSystemDomain;
using TaggedImageViewer.ImageProcessingDomain;
using TaggedImageViewer.Utils;
using TaggedImageViewer.ViewModels;

namespace TaggedImageViewer;

public partial class App
{
    private static IServiceProvider? ServiceProvider { get; set; }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureAppSettings(serviceCollection, "appsettings.json");
        ConfigureServices(serviceCollection);
        
        ServiceProvider = serviceCollection.BuildServiceProvider(); 
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureAppSettings(IServiceCollection services, string fileName)
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile(fileName, optional: true, reloadOnChange: true)
            .Build();
        
        Settings settings = new();
        config.Bind(settings);

        settings.PropertyChanged += (sender, args) =>
        {
            // workaround for IConfiguration being only a one-way binding from file to object instance
            // does not scale well, but good enough for now
            string json = JsonSerializer.Serialize(settings);
            File.WriteAllText(fileName, json);
        };

        services.AddSingleton(settings);
    }
    
    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IImageService, ImageProcessingService>();
        services.AddSingleton<IDirectoryService, DrawingsDirectoryService>();
        services.AddTransient<MainWindow>();
    }
}