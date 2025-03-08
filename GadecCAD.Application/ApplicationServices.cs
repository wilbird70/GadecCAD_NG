using GadecCAD.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GadecCAD.Application;
public static class ApplicationServices
{
    private static ServiceProvider _serviceProvider = default!;

    public static void Config()
    {
        var serviceCollection = new ServiceCollection()
            .AddTransient(typeof(XmlService<>))
            .AddTransient<FrameSetService>()
            .AddTransient<IDrawingDataService, DrawingDataService>()
            .AddTransient<FrameInfoService>();

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
