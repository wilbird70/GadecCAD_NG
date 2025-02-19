using System.Text.RegularExpressions;

namespace GadecLibrary.Constants;
public static partial class RegularExpressions
{
    [GeneratedRegex(@"\d+")] private static partial Regex GetNumbersRegex();
    public static readonly Regex GetNumbers = GetNumbersRegex();

    [GeneratedRegex(@"[a-zA-Z](?=[^a-zA-Z]*$)")] private static partial Regex GetLastLetterRegex();
    public static readonly Regex GetLastLetter = GetLastLetterRegex();
}
