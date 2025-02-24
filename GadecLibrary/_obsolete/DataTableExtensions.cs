using GadecLibrary._obsolete;
using GadecLibrary.Extensions;
using System.Data;

namespace GadecLibrary._obsolete;
public static class DataTableExtensions
{
    public static string[] GetColumnNames(this DataTable eDataTable)
    {
        if (eDataTable is null)
            return [];

        return eDataTable.Columns.ToArray().Select(e => e.ColumnName).ToArray();
    }

    public static DataTable? GetTable(this DataTable eDataTable, string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            return null;

        if (eDataTable.DataSet?.Tables.Contains(tableName) is not true)
            return new DataTable(tableName);

        return eDataTable.DataSet.Tables[tableName];
    }

    public static void AssignPrimaryKey(this DataTable eDataTable, string primaryKeyString)
    {
        var primaryKeyColumns = GetColumnList(eDataTable, primaryKeyString);

        if (primaryKeyColumns.Count > 0)
        {
            eDataTable.PrimaryKey = [.. primaryKeyColumns];
        }
    }

    // Nog testen maken...
    public static void AssignDefaultViewSort(this DataTable eDataTable, string sortColumnString)
    {
        var sortColumns = GetColumnList(eDataTable, sortColumnString);

        if (sortColumns.Count > 0)
        {
            eDataTable.DefaultView.Sort = string.Join(",", sortColumns);
        }
    }

    private static List<DataColumn> GetColumnList(DataTable eDataTable, string columnString)
    {
        if (string.IsNullOrWhiteSpace(columnString))
            return [];

        var sortColumns = new List<DataColumn>();
        foreach (var columnName in columnString.Cut())
        {
            if (string.IsNullOrWhiteSpace(columnName))
                continue;

            var column = eDataTable.Columns[columnName]
                ?? throw new ColumnNotFoundException(columnName, eDataTable);

            sortColumns.Add(column);
        }
        return sortColumns;
    }
}
