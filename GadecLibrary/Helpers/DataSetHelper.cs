using GadecLibrary.Extensions;
using System.Data;

namespace GadecLibrary.Helpers;
public class DataSetHelper
{

    /// <summary>
    /// Loads a database collection from the file.
    /// </summary>
    /// <param name="file">The full filename of the xml-file.</param>
    /// <returns>The database collection.</returns>
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
