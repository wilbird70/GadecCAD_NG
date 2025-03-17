using System.Globalization;

namespace Gadec.Common.Helpers;
public static class DateOnlyHelper
{
    public static string? ToString(DateOnly? date) => date == DateOnly.MinValue ? null : date?.ToString("dd-MM-yyyy");

    public static DateOnly? FromString(string? dateString)
    {
        if (DateOnly.TryParseExact(dateString, ["d-M-yyyy", "d/M/yyyy"], CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateOnly result))
        {
            return result;
        }
        return null;
    }
}
