using Autodesk.AutoCAD.DatabaseServices;

namespace GadecCAD.Application.Extensions;
public static class TransactionExtensions
{
    public static DBDictionary? GetDBDictionary(this Transaction eTransaction, ObjectId objectId, OpenMode openMode = OpenMode.ForRead)
    {
        try
        { return (DBDictionary)eTransaction.GetObject(objectId, openMode); }
        catch
        { return null; }
    }

    public static Xrecord? GetXRecord(this Transaction eTransaction, ObjectId objectId, OpenMode openMode = OpenMode.ForRead)
    {
        try
        { return (Xrecord)eTransaction.GetObject(objectId, openMode); }
        catch
        { return null; }
    }

    public static BlockReference? GetBlockReference(this Transaction eTransaction, ObjectId objectId, OpenMode openMode = OpenMode.ForRead)
    {
        try
        { return (BlockReference)eTransaction.GetObject(objectId, openMode); }
        catch
        { return null; }
    }

    public static AttributeReference? GetAttributeReference(this Transaction eTransaction, ObjectId objectId, OpenMode openMode = OpenMode.ForRead)
    {
        try
        { return (AttributeReference)eTransaction.GetObject(objectId, openMode); }
        catch
        { return null; }
    }
}
