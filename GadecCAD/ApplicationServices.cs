using GadecCAD.Application.Handlers;
using GadecCAD.Application.Interfaces;
using GadecCAD.Data.Services;
using GadecCAD.Infrastructure.AutoCADServices;
using GadecCAD.Infrastructure.Services;
using GadecCAD.Middleware;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GadecCAD;
public static class ApplicationServices
{
    private static ServiceProvider _serviceProvider = default!;

    public static void AddServices()
    {
        var services = new ServiceCollection()
            .AddTransient<FrameInfoService>()
            .AddTransient<IDateTimeService, DateTimeService>()
            .AddTransient<IDrawingDataService, DrawingDataService>()
            .AddTransient<IEditorService, EditorService>()
            .AddTransient<IFileSystemService, FileSystemService>()
            .AddTransient(typeof(IXmlService<>), typeof(XmlService<>))
            .AddMediatR(config => config.RegisterServicesFromAssemblies(typeof(UpdateDrawingListHandler).Assembly))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

        _serviceProvider = services.BuildServiceProvider();
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
