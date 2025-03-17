using Gadec.Common.Helpers;

namespace Gadec.Common.Tests.Helpers;
internal class DateOnlyHelperTests
{

    private static readonly object[] _testCaseSource_date_only_to_string = new[]
    {
        new object[] { DateOnly.MaxValue, "31-12-9999" },
        [null!, null! ],
        [DateOnly.MinValue, null!],
        [new DateOnly(2025, 2, 1), "01-02-2025"],
        [new DateOnly(2025, 3, 8), "08-03-2025"],
        [new DateOnly(2025, 12, 31), "31-12-2025"],
    };

    [TestCaseSource(nameof(_testCaseSource_date_only_to_string))]
    public void Test_date_only_to_string(DateOnly date, string expectedValue)
    {
        var dateString = DateOnlyHelper.ToString(date);

        Assert.That(dateString, Is.EqualTo(expectedValue));
    }

    private static readonly object[] _testCaseSource_date_only_from_string = new[]
    {
        new object[] { "31 - 12 - 9999", DateOnly.MaxValue },
        [null!, null!],
        [string.Empty, null!],
        ["01-02-2025 ", new DateOnly(2025, 2, 1)],
        [" 8-3-2025", new DateOnly(2025, 3, 8)],
        [" 31 - 12 - 2025 ", new DateOnly(2025, 12, 31)],
        ["11/ 9  /2024", new DateOnly(2024, 9, 11)],
    };

    [TestCaseSource(nameof(_testCaseSource_date_only_from_string))]
    public void Test_date_only_from_string(string? dateString, DateOnly? expectedValue)
    {
        var date = DateOnlyHelper.FromString(dateString);

        Assert.That(date, Is.EqualTo(expectedValue));
    }
}
