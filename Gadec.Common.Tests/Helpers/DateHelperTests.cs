using Gadec.Common.Helpers;

namespace Gadec.Common.Tests.Helpers;
internal class DateHelperTests
{
    private static readonly object[] _testCaseSource_date_time_to_string = new[]
    {
        new object[] { DateTime.MaxValue, "9999-12-31 23:59:59" },
        [new DateTime(2025, 3, 17, 19, 47, 34), "2025-03-17 19:47:34"],
    };

    [TestCaseSource(nameof(_testCaseSource_date_time_to_string))]
    public void Test_date_time_to_string(DateTime date, string expectedValue)
    {
        var dateString = DateHelper.ToString(date);

        Assert.That(dateString, Is.EqualTo(expectedValue));
    }

    private static readonly object[] _testCaseSource_date_time_from_string = new[]
    {
        new object[] { "9999-12-31 23:59:59", new DateTime(9999,12, 31, 23, 59, 59) },
        ["2025-03-17 19:47:34", new DateTime(2025, 3, 17, 19, 47, 34)],
    };

    [TestCaseSource(nameof(_testCaseSource_date_time_from_string))]
    public void Test_date_time_from_string(string dateString, DateTime? expectedValue)
    {
        var date = DateHelper.DateTimeFromString(dateString);

        Assert.That(date, Is.EqualTo(expectedValue));
    }

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
        var dateString = DateHelper.ToString(date);

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
        var date = DateHelper.DateOnlyFromString(dateString);

        Assert.That(date, Is.EqualTo(expectedValue));
    }
}
