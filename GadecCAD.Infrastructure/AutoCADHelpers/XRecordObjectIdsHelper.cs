using Autodesk.AutoCAD.DatabaseServices;
using GadecCAD.Infrastructure.AutoCADExtensions;

namespace GadecCAD.Infrastructure.AutoCADHelpers;
public static class XRecordObjectIdsHelper
{
    public static Dictionary<string, ObjectIdCollection> Load(Database? database, string sectionName)
    {
        var sectionId = XRecordHelper.GetSectionId(database, sectionName, false);
        if (database is null || sectionId.IsNull)
            return [];

        var result = new Dictionary<string, ObjectIdCollection>();

        using var tr = database.TransactionManager.StartTransaction();
        var section = tr.GetDBDictionary(sectionId)!;
        foreach (var entry in section)
        {
            var xRecord = tr.GetXRecord(section.GetAt(entry.Key))!;
            var frameIds = new ObjectIdCollection();
            foreach (var typedValue in xRecord.Data)
            {
                frameIds.Add((ObjectId)typedValue.Value);
            }
            result.TryAdd(entry.Key, frameIds);
        }
        tr.Commit();
        return result;
    }
}
