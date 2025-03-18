using Gadec.Common.Constants;
using System.Text.RegularExpressions;

namespace Gadec.Common.Tests.Constants;
internal class RegularExpressionsTests
{
    [TestCase("2345A", true, "A")]
    [TestCase("2345", false, "")]
    [TestCase("23Goe4d5", true, "d")]
    [TestCase("Gieren", true, "n")]
    public void Test_GetLastLetter(string inputString, bool expectedSuccess, string expectedResult)
    {
        Match match = RegularExpressions.GetLastLetter.Match(inputString);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(match.Success, Is.EqualTo(expectedSuccess));
            Assert.That(match.Value, Is.EqualTo(expectedResult));
        }
    }

    [TestCase("A001", 1, new[] { "001" })]
    [TestCase("X1-34", 2, new[] { "1", "34" })]
    [TestCase("23Goe4d5", 3, new[] { "23", "4", "5" })]
    public void Test_GetNumbers(string inputString, int expectedCount, string[] expectedResult)
    {
        MatchCollection matches = RegularExpressions.GetNumbers.Matches(inputString);
        var values = matches.Select(e => e.Value);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(matches, Has.Count.EqualTo(expectedCount));
            Assert.That(values, Is.EquivalentTo(expectedResult));
        }
    }

    [Test]
    public void Test_GetNumbers_from_string_without_numbers_returns_empty_string_array()
    {
        MatchCollection matches = RegularExpressions.GetNumbers.Matches("GierenKunnenGoedVliegenDatIsWaarNietWaarIsDatVliegenGoedKunnenGieren");
        var values = matches.Select(e => e.Value);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(matches, Has.Count.EqualTo(0));
            Assert.That(values, Is.EquivalentTo(Array.Empty<string>()));
        }
    }
}
