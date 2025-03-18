using Gadec.Common.Extensions;

namespace Gadec.Common.Tests.Extensions;
internal class StringExtensionsTests
{
    [TestCase("GierenKunnenGoedVliegenDatIsWaarNietWaarIsDatVliegenGoedKunnenGieren", "Kunnen", "Waar", "Error", false, "GoedVliegenDatIs")]
    [TestCase("GierenKunnenGoedVliegenDatIsWaarNietWaarIsDatVliegenGoedKunnenGieren", "Kunnen", "Waar", "Error", true, "KunnenGoedVliegenDatIsWaar")]
    [TestCase("GierenKunnenGoedVliegenDatIsWaarNietWaarIsDatVliegenGoedKunnenGieren", "Kunnen", "", "Error", false, "GoedVliegenDatIsWaarNietWaarIsDatVliegenGoedKunnenGieren")]
    [TestCase("GierenKunnenGoedVliegenDatIsWaarNietWaarIsDatVliegenGoedKunnenGieren", "Kunnen", "", "Error", true, "KunnenGoedVliegenDatIsWaarNietWaarIsDatVliegenGoedKunnenGieren")]
    [TestCase("GierenKunnenGoedVliegenDatIsWaarNietWaarIsDatVliegenGoedKunnenGieren", "Wespen", "Waar", "Error", false, "Error")]
    [TestCase("GierenKunnenGoedVliegenDatIsWaarNietWaarIsDatVliegenGoedKunnenGieren", "Kunnen", "Wespen", "Error", true, "Error")]
    public void Test_InStrResult(string inputString, string startsAfter, string endsBefore, string noResultString, bool includeSearchStrings, string expectedResult)
    {
        var result = inputString.InStrResult(startsAfter, endsBefore, noResultString, includeSearchStrings);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase("GierenKunnenGoedVliegenDatIsWaarNietWaarIsDatVliegenGoedKunnenGieren", "Waar", "Kunnen", "Error", false, "IsDatVliegenGoed")]
    [TestCase("GierenKunnenGoedVliegenDatIsWaarNietWaarIsDatVliegenGoedKunnenGieren", "Waar", "Kunnen", "Error", true, "WaarIsDatVliegenGoedKunnen")]
    [TestCase("GierenKunnenGoedVliegenDatIsWaarNietWaarIsDatVliegenGoedKunnenGieren", "Waar", "", "Error", false, "IsDatVliegenGoedKunnenGieren")]
    [TestCase("GierenKunnenGoedVliegenDatIsWaarNietWaarIsDatVliegenGoedKunnenGieren", "Waar", "", "Error", true, "WaarIsDatVliegenGoedKunnenGieren")]
    [TestCase("GierenKunnenGoedVliegenDatIsWaarNietWaarIsDatVliegenGoedKunnenGieren", "Wespen", "Kunnen", "Error", false, "Error")]
    [TestCase("GierenKunnenGoedVliegenDatIsWaarNietWaarIsDatVliegenGoedKunnenGieren", "Waar", "Wespen", "Error", true, "Error")]
    public void Test_InStrRevResult(string inputString, string startsAfter, string endsBefore, string noResultString, bool includeSearchStrings, string expectedResult)
    {
        var result = inputString.InStrRevResult(startsAfter, endsBefore, noResultString, includeSearchStrings);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    // TODO Test for Replace (multiple)

    [TestCase("ABC", "ABC1")]
    [TestCase("A0", "A1")]
    [TestCase("A9", "A10")]
    [TestCase("A001", "A002")]
    [TestCase("A000345", "A000346")]
    [TestCase("P4A01", "P4A02")]
    [TestCase("P4A01X", "P4A02X")]
    public void Test_AutoNumber(string inputString, string expectedResult)
    {
        var result = inputString.AutoNumber();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase("ABC", null, "ABC1")]
    [TestCase("A0", null, "A1")]
    [TestCase("A009", 5, "A014")]
    [TestCase("A009", -5, "A004")]
    [TestCase("A004", -5, "A-01")]
    [TestCase("A000345", 8, "A000353")]
    public void Test_AddNumber(string inputString, int? add, string expectedResult)
    {
        var result = inputString.AddNumber(add);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase("123abc!", "c")]
    [TestCase("Hallo wereld!!", "d")]
    [TestCase("TesT1234", "T")]
    [TestCase("4567!?", "")]
    public void Test_LastLetter(string inputString, string expectedResult)
    {
        var result = inputString.LastLetter();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase("123abc!", true)]
    [TestCase("Hallo wereld!!", false)]
    [TestCase("TesT1234", true)]
    [TestCase("4567!?", true)]
    public void Test_HasNumber(string inputString, bool expectedResult)
    {
        var result = inputString.HasNumber();

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase("A009", 1, "009")]
    [TestCase("A009", 2, "09")]
    [TestCase("A004", 3, "4")]
    [TestCase("A000345", 7, "")]
    public void Test_EraseStart(string inputString, int length, string expectedResult)
    {
        var result = inputString.EraseStart(length);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase("A009", 1, "A00")]
    [TestCase("A009", 2, "A0")]
    [TestCase("A000345", 7, "")]
    [TestCase("A000345", 8, "")]
    public void Test_EraseEnd(string inputString, int length, string expectedResult)
    {
        var result = inputString.EraseEnd(length);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase("123abc!", null)]
    [TestCase("Hallo wereld!!", null)]
    [TestCase("1234", 1234)]
    [TestCase("45,67", 45.67)]
    [TestCase("45.67", 45.67)]
    public void Test_ToDouble(string inputString, double? expectedResult)
    {
        var result = inputString.ToDouble();

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase("123abc!", 0)]
    [TestCase("Hallo wereld!!", 0)]
    [TestCase("1234", 1234)]
    [TestCase("45,67", 0)]
    public void Test_ToInteger(string inputString, double expectedResult)
    {
        var result = inputString.ToInteger();

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase("GierenKunnenGoedVliegen", 0, 71)]
    [TestCase("GierenKunnenGoedVliegen", 2, 101)]
    [TestCase("GierenKunnenGoedVliegen", 6, 75)]
    [TestCase("GierenKunnenGoedVliegen", 25, 0)]
    public void Test_GetAscii(string inputString, int position, int expectedResult)
    {
        var result = inputString.GetAscii(position);

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase("GierenKunnenGoedVliegen", 0, 'G')]
    [TestCase("GierenKunnenGoedVliegen", 2, 'e')]
    [TestCase("GierenKunnenGoedVliegen", 6, 'K')]
    [TestCase("GierenKunnenGoedVliegen", 25, null)]
    public void Test_GetChar(string inputString, int position, char? expectedResult)
    {
        var result = inputString.GetChar(position);

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase('G', 71)]
    [TestCase('e', 101)]
    [TestCase('K', 75)]
    [TestCase(null, 0)]
    public void Test_GetAscii(char? inputString, int expectedResult)
    {
        var result = inputString.GetAscii();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}
