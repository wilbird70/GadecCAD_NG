using GadecLibrary._obsolete;
using System.Data;

namespace GadecLibrary._obsolete;
public static class ToArrayExtensions
{
    public static DataRow[] ToArray(this DataRowCollection eCollection) => eCollection.Cast<DataRow>().ToArray();
    public static DataColumn[] ToArray(this DataColumnCollection eCollection) => eCollection.Cast<DataColumn>().ToArray();
}
