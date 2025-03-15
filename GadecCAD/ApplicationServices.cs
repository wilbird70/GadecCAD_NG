using GadecCAD.Application.Handlers;
using GadecCAD.Application.Interfaces;
using GadecCAD.Data.Services;
using GadecCAD.Infrastructure.AutoCADServices;
using GadecCAD.Infrastructure.Services;
using GadecCAD.Middleware;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using System.Reflection;

namespace GadecCAD;
public static class ApplicationServices
{
    private static ServiceProvider _serviceProvider = default!;

    public static void AddServices()
    {
        var services = new ServiceCollection()
            .AddMediatR(config => config.RegisterServicesFromAssemblies(typeof(UpdateDrawingListHandler).Assembly))
            .AddSingleton(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>))
            .AddCustomServices()
            .AddLoggingService();

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

    private static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.AddTransient<FrameInfoService>()
                .AddTransient<IDateTimeService, DateTimeService>()
                .AddTransient<IDrawingDataService, DrawingDataService>()
                .AddTransient<IEditorService, EditorService>()
                .AddTransient<IFileSystemService, FileSystemService>()
                .AddTransient(typeof(IXmlService<>), typeof(XmlService<>));

        return services;
    }

    private static IServiceCollection AddLoggingService(this IServiceCollection services)
    {
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            loggingBuilder.AddNLog(new NLogProviderOptions
            {
                CaptureMessageTemplates = true,
                CaptureMessageProperties = true
            });
        });

        var pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? throw new ArgumentException("Application Plugin folder not found.");
        LogManager.Setup().LoadConfigurationFromFile(Path.Combine(pluginFolder, "NLog.config.xml"));

        var logFileName = Path.Combine(pluginFolder, "Logs", $"Gadec_AutoCAD_Log.{DateTime.Now:yyyy-MM-dd}.log");
        LogManager.Configuration.Variables["LogFileName"] = logFileName;
        LogManager.ReconfigExistingLoggers();

        return services;
    }
}
