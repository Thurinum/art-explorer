using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TaggedImageViewer.FileSystemDomain;
using TaggedImageViewer.ImageProcessingDomain;
using TaggedImageViewer.Utils;

namespace TaggedImageViewer;

public partial class App
{
    private static IServiceProvider? ServiceProvider { get; set; }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        
        ServiceProvider = serviceCollection.BuildServiceProvider(); 
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
    
    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<IImageService, ImageProcessingService>();
        services.AddSingleton<IDirectoryService, DrawingsDirectoryService>();
        services.AddTransient<MainWindow>();
    }
}