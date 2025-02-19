using Autodesk.AutoCAD.ApplicationServices;

namespace GadecCAD.Extensions;
public static class ToListExtensions
{
    public static List<Document> ToList(this DocumentCollection collection) => collection.Cast<Document>().ToList();
}
