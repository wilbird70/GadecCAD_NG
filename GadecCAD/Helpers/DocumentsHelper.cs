using Autodesk.AutoCAD.ApplicationServices;
using GadecCAD.Extensions;

namespace GadecCAD.Helpers;
public static class DocumentsHelper
{
    public static Dictionary<string, Document> GetOpenDocuments()
    {
        var result = new Dictionary<string, Document>();
        foreach (var document in Application.DocumentManager.ToList())
        {
            document.WasClosed(false);
            result.TryAdd(document.Name, document);
        }
        return result;
    }
}
