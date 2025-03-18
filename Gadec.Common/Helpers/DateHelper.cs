using System.Globalization;

namespace Gadec.Common.Helpers;
public static class DateHelper
{
    public static string ToString(DateTime date) => date.ToString("yyyy-MM-dd HH:mm:ss");
    public static string? ToString(DateOnly? date) => date == DateOnly.MinValue ? null : date?.ToString("dd-MM-yyyy");

    public static DateTime DateTimeFromString(string dateString)
    {
        return DateTime.ParseExact(dateString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
    }

    public static DateOnly? DateOnlyFromString(string? dateString)
    {
        if (DateOnly.TryParseExact(dateString, ["d-M-yyyy", "d/M/yyyy"], CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateOnly result))
        {
            return result;
        }
        return null;
    }
}
