using GadecCAD.Application.Interfaces;
using GadecCAD.Application.Services;
using GadecCAD.Data.Services;
using GadecCAD.Infrastructure.AutoCADServices;
using GadecCAD.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GadecCAD;
public static class ApplicationServices
{
    private static ServiceProvider _serviceProvider = default!;

    public static void Config()
    {
        var serviceCollection = new ServiceCollection()
            .AddTransient(typeof(XmlService<>))
            .AddTransient<FrameSetService>()
            .AddTransient<IDrawingDataService, DrawingDataService>()
            .AddTransient<FrameInfoService>()
            .AddTransient<IFileSystemService, FileSystemService>();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    public static T GetRequiredService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    public static void ShutDown()
    {
        _serviceProvider?.Dispose();
    }
}
