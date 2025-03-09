using Autodesk.AutoCAD.DatabaseServices;
using GadecCAD.Application.Constants;
using GadecCAD.Infrastructure.AutoCADExtensions;

namespace GadecCAD.Infrastructure.AutoCADHelpers;
public static class XRecordHelper
{
    public static ObjectId GetSectionId(Database? database, string sectionName, bool createIfNotExisting)
    {
        var result = ObjectId.Null;
        if (database?.TransactionManager is null)
            return result;

        using var tr = database.TransactionManager.StartTransaction();
        var namedObjectsDictionary = tr.GetDBDictionary(database.NamedObjectsDictionaryId)!;
        if (namedObjectsDictionary.Contains(AppConstants.Company))
        {
            var companyDictionary = tr.GetDBDictionary(namedObjectsDictionary.GetAt(AppConstants.Company))!;
            if (companyDictionary.Contains(sectionName))
            {
                result = companyDictionary.GetAt(sectionName);
            }
            else if (createIfNotExisting)
            {
                var sectionDictionary = new DBDictionary();
                companyDictionary!.UpgradeOpen();
                result = companyDictionary.SetAt(sectionName, sectionDictionary);
                tr.AddNewlyCreatedDBObject(sectionDictionary, true);
            }
        }
        else if (createIfNotExisting)
        {
            var companyDictionary = new DBDictionary();
            namedObjectsDictionary.UpgradeOpen();
            namedObjectsDictionary.SetAt(AppConstants.Company, companyDictionary);
            tr.AddNewlyCreatedDBObject(companyDictionary, true);
            var sectionDictionary = new DBDictionary();
            result = companyDictionary.SetAt(sectionName, sectionDictionary);
            tr.AddNewlyCreatedDBObject(sectionDictionary, true);
        }
        tr.Commit();
        return result;
    }
}
