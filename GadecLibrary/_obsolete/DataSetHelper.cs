using GadecLibrary.Extensions;
using System.Data;
namespace GadecLibrary._obsolete;

public static class DataSetHelper
{
    public static DataSet? LoadFromXml(string file)
    {
        if (!File.Exists(file))
            return new DataSet();

        try
        {
            var output = new DataSet("Help");
            output.ReadXml(file);
            return output;
        }
        catch (Exception ex)
        {
            ex.AddData($"FileName: {file}");
            ex.Rethrow();
            return null;
        }
    }
}
