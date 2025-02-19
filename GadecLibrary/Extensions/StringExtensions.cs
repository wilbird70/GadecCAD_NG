using GadecLibrary.Constants;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GadecLibrary.Extensions;
public static class StringExtensions
{
    public static string InStrResult(this string eString, string startsAfter, string endsBefore = "", string noResultString = "", bool includeSearchStrings = false)
    {
        var startsAt = eString.IndexOf(startsAfter);

        if (startsAt == -1)
            return noResultString;

        if (startsAfter.Length == 0)
            startsAt = 0;

        startsAt += startsAfter.Length;

        if (endsBefore.Length == 0)
            return includeSearchStrings ? $"{startsAfter}{eString[startsAt..]}" : eString[startsAt..];

        var endsAt = eString.IndexOf(endsBefore, startsAt);

        if (endsAt == -1)
            return noResultString;

        var result = eString[startsAt..endsAt];

        return includeSearchStrings ? $"{startsAfter}{result}{endsBefore}" : result;
    }

    public static string InStrRevResult(this string eString, string startsAfter, string endsBefore = "", string noResultString = "", bool includeSearchStrings = false)
    {
        var startsAt = eString.LastIndexOf(startsAfter);

        if (startsAt == -1)
            return noResultString;

        if (startsAfter.Length == 0)
            startsAt = 0;

        startsAt += startsAfter.Length;

        if (endsBefore.Length == 0)
            return includeSearchStrings ? $"{startsAfter}{eString[startsAt..]}" : eString[startsAt..];

        var endsAt = eString.IndexOf(endsBefore, startsAt);

        if (endsAt == -1)
            return noResultString;

        var result = eString[startsAt..endsAt];

        return includeSearchStrings ? $"{startsAfter}{result}{endsBefore}" : result;
    }

    public static string[] Cut(this string eString, string? delimiter = null) => eString.Split(delimiter ?? ";");

    public static string Item(this string[] eStrings, int index) => eStrings.Length > index ? eStrings[index] : string.Empty;

    public static string ReplaceMultiple(this string eString, Dictionary<string, string> searchAndReplaceValues)
    {
        var result = eString;
        foreach (var searchAndReplaceValue in searchAndReplaceValues)
        {
            result = result.Replace(searchAndReplaceValue.Key, searchAndReplaceValue.Value);
        }
        return result;
    }

    public static string AutoNumber(this string eString) => eString.AddNumber();

    public static string AddNumber(this string eString, int? add = null)
    {
        var parsed = eString.GetLastNumber();
        if (parsed.Number is not null)
        {
            var numberResult = parsed.Number + (add ?? 1);
            var numberString = Math.Abs(numberResult.Value).ToString();
            var numberLength = numberString.Length;
            var length = numberLength > parsed.Match.Length ? numberLength : parsed.Match.Length;
            var negativeSign = numberResult < 0 ? "-" : string.Empty;
            var result = numberString.PadLeft(length - negativeSign.Length, '0');
            return $"{parsed.Before}{negativeSign}{result}{parsed.After}";
        }

        if (parsed.Number is null && add is null)
            return $"{parsed.Before}1{parsed.After}";

        return eString;
    }

    public static string LastLetter(this string eString)
    {
        Match match = RegularExpressions.GetLastLetter.Match(eString);
        if (!match.Success)
            return string.Empty;

        return match.Value;
    }

    public static bool HasNumber(this string eString) => RegularExpressions.GetNumbers.Match(eString).Success;

    public static string EraseStart(this string eString, int length) => length < eString.Length ? eString[length..] : string.Empty;

    public static string EraseEnd(this string eString, int length) => length < eString.Length ? eString[..^length] : string.Empty;

    public static string LeftString(this string eString, int length) => length < eString.Length ? eString[..length] : eString;

    public static string RightString(this string eString, int length) => length < eString.Length ? eString[^length..] : eString;

    public static string MidString(this string eString, int start, int? length = null)
    {
        if (length is null || start + length > eString.Length)
            return start < eString.Length ? eString[start..] : string.Empty;

        return eString.Substring(start, length.Value);
    }

    public static double? ToDouble(this string eString)
    {
        if (double.TryParse(eString.Replace(",", "."), CultureInfo.InvariantCulture, out var result))
            return result;

        return null;
    }

    public static double ToInteger(this string eString)
    {
        if (int.TryParse(eString, out var result))
            return result;

        return 0;
    }

    public static int GetAscii(this string eString, int position) => eString.GetChar(position).GetAscii();

    public static int GetAscii(this char? eString) => eString is null ? 0 : (int)eString;

    public static char? GetChar(this string eString, int position)
    {
        if (char.TryParse(eString.MidString(position, 1), out var result))
            return result;

        return null;
    }

    /// <summary> Obsolete extension </summary>
    public static (string Prefix, string Result, string Suffix) FindResultAsPattern(this string eString, string pattern)
        => throw new NotImplementedException("Obsolete extension");

    /// <summary> Obsolete extension </summary>
    public static (string Prefix, string Result, string Suffix) FindResultAsPatternRev(this string eString, string pattern)
        => throw new NotImplementedException("Obsolete extension");

    private static (int? Number, string Before, string Match, string After) GetLastNumber(this string eString)
    {
        MatchCollection matches = RegularExpressions.GetNumbers.Matches(eString);
        if (matches.Count == 0)
            return (null, eString, string.Empty, string.Empty);

        Match lastMatch = matches[^1];
        int? number = int.Parse(lastMatch.Value);
        string before = eString[..lastMatch.Index];
        string match = lastMatch.Value;
        string after = eString[(lastMatch.Index + lastMatch.Length)..];
        return (number, before, match, after);
    }
}
