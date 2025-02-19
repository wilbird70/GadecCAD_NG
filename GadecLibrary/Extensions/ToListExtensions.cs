using System.Data;

namespace GadecLibrary.Extensions;
public static class ToListExtensions
{
    public static List<DataRow> ToList(this DataRowCollection eCollection) => eCollection.Cast<DataRow>().ToList();
    public static List<DataColumn> ToList(this DataColumnCollection eCollection) => eCollection.Cast<DataColumn>().ToList();
}
