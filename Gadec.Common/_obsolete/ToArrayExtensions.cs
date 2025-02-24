using System.Data;

namespace Gadec.Common._obsolete;
public static class ToArrayExtensions
{
    public static DataRow[] ToArray(this DataRowCollection eCollection) => eCollection.Cast<DataRow>().ToArray();
    public static DataColumn[] ToArray(this DataColumnCollection eCollection) => eCollection.Cast<DataColumn>().ToArray();
}
