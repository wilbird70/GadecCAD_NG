using Autodesk.AutoCAD.ApplicationServices;

namespace GadecCAD.Infrastructure.AutoCADExtensions;
public static class DocumentExtensions
{
    public static List<Document> Documents(this DocumentCollection documentManager) => documentManager.Cast<Document>().ToList();

    public static bool WasClosed(this Document document, bool? newValue = null)
    {
        if (newValue is not null)
        {
            if (document.UserData.ContainsKey("WasClosed"))
            {
                document.UserData["WasClosed"] = newValue;
            }
            else
            {
                document.UserData.Add("WasClosed", newValue);
            }
        }

        if (document.UserData.ContainsKey("WasClosed"))
            return (bool)(document.UserData["WasClosed"] ?? false);

        return false;
    }
}
