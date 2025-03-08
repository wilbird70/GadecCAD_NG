using Autodesk.AutoCAD.ApplicationServices;
using GadecCAD.Application.Extensions;
using AutoCAD = Autodesk.AutoCAD.ApplicationServices.Application;

namespace GadecCAD.Application.Helpers;
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
