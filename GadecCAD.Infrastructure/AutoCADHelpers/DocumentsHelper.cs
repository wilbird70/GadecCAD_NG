using Autodesk.AutoCAD.ApplicationServices;
using GadecCAD.Infrastructure.AutoCADExtensions;
using AutoCAD = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace GadecCAD.Infrastructure.AutoCADHelpers;
public static class DocumentsHelper
{
    public static Dictionary<string, Document> GetOpenDocuments()
    {
        var result = new Dictionary<string, Document>();
        foreach (Document document in AutoCAD.DocumentManager)
        {
            document.WasClosed(false);
            result.TryAdd(document.Name, document);
        }
        return result;
    }
}
