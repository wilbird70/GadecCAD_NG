using GadecCAD.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GadecCAD.Application;
public static class AppServices
{
    private static ServiceProvider _serviceProvider = default!;

    public static void Config()
    {
        var serviceCollection = new ServiceCollection()
            .AddTransient<FrameSetService>()
            .AddTransient<XmlService>();

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
