using Gadec.Common.Extensions;
using System.Data;

namespace Gadec.Common._obsolete;
public static class DataRowExtensions
{
    public static void SetString(this DataRow eDataRow, string columnName, string text)
    {
        if (eDataRow.Table.Columns.Contains(columnName))
        {
            eDataRow[columnName] = text;
            return;
        }

        eDataRow.Table.Columns.Add(new DataColumn(columnName, typeof(string), "", MappingType.Attribute));
        eDataRow[columnName] = text;
    }

    public static string[] GetColumnNames(this DataRow eDataRow)
    {
        return eDataRow.Table.GetColumnNames();
    }

    public static object? GetValue(this DataRow eDataRow, string columnName) => eDataRow.HasValue(columnName) ? eDataRow[columnName] : null;

    public static string GetString(this DataRow eDataRow, string columnName)
    {
        var value = eDataRow.GetValue(columnName);
        if (value is string stringValue)
            return stringValue;

        return string.Empty;
    }

    public static double? GetDouble(this DataRow eDataRow, string columnName)
    {
        var value = eDataRow.GetValue(columnName);

        if (value is null)
            return null;

        if (value is double doubleValue)
            return doubleValue;

        if (value is string stringValue)
            return stringValue.ToDouble();

        return null;
    }

    public static bool HasValue(this DataRow eDataRow, string columnName)
        => eDataRow.Table.Columns.Contains(columnName) && !Convert.IsDBNull(eDataRow[columnName]);
}
