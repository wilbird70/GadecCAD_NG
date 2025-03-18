using Autodesk.AutoCAD.ApplicationServices;
using AutoCAD = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace GadecCAD.Events;
public static class ApplicationEvents
{
    public static void Initialize()
    {
        AutoCAD.QuitWillStart += QuitWillStartEventHandler;
        AutoCAD.QuitAborted += QuitAbortedEventHandler;
        AutoCAD.SystemVariableChanged += SystemVariableChangedEventHandler;
        var dm = AutoCAD.DocumentManager;
        dm.DocumentCreated += DocumentCreatedEventHandler;
        dm.DocumentActivated += DocumentActivatedEventHandler;
        dm.DocumentDestroyed += DocumentDestroyedEventHandler;
        dm.DocumentToBeDestroyed += DocumentToBeDestroyedEventHandler;
    }

    private static void QuitWillStartEventHandler(object? sender, EventArgs e)
    {
    }

    private static void QuitAbortedEventHandler(object? sender, EventArgs e)
    {
    }

    private static void SystemVariableChangedEventHandler(object? sender, EventArgs e)
    {
    }

    // Document

    private static void DocumentCreatedEventHandler(object? sender, DocumentCollectionEventArgs e)
    {
    }

    private static void DocumentActivatedEventHandler(object? sender, DocumentCollectionEventArgs e)
    {
    }

    private static void DocumentDestroyedEventHandler(object? sender, DocumentDestroyedEventArgs e)
    {
    }

    private static void DocumentToBeDestroyedEventHandler(object? sender, DocumentCollectionEventArgs e)
    {
    }
}
