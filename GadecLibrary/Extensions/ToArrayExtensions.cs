using System.Data;

namespace GadecLibrary.Extensions;
public static class ToArrayExtensions
{
    public static DataRow[] ToArray(this DataRowCollection eCollection) => eCollection.Cast<DataRow>().ToArray();
    public static DataColumn[] ToArray(this DataColumnCollection eCollection) => eCollection.Cast<DataColumn>().ToArray();
}
