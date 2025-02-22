using System.Data;

namespace GadecLibrary._obsolete;

public class ColumnNotFoundException : Exception
{
    public ColumnNotFoundException() : base()
    {
    }

    public ColumnNotFoundException(string? message) : base(message)
    {
    }

    public ColumnNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public ColumnNotFoundException(string? columnName, DataTable dataTable) : base($"Column '{columnName}' not found in table '{dataTable.TableName}'")
    {
    }
}
