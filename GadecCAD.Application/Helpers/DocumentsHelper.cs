using GadecCAD.Application.Extensions;
using Acad = Autodesk.AutoCAD.ApplicationServices;

namespace GadecCAD.Application.Helpers;
public static class DocumentsHelper
{
    public static Dictionary<string, Acad.Document> GetOpenDocuments()
    {
        var result = new Dictionary<string, Acad.Document>();
        foreach (Acad.Document document in Acad.Application.DocumentManager)
        {
            document.WasClosed(false);
            result.TryAdd(document.Name, document);
        }
        return result;
    }
}
