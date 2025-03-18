using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using GadecCAD.Infrastructure.AutoCADExtensions;
using AutoCAD = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace GadecCAD.Infrastructure.AutoCADHelpers;
public static class DocumentsHelper
{
    public static Document ActiveDocument => AutoCAD.DocumentManager.MdiActiveDocument;
    public static Editor ActiveEditor => AutoCAD.DocumentManager.MdiActiveDocument.Editor;

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
