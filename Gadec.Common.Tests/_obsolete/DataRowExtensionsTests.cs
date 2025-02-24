using Gadec.Common._obsolete;
using System.Data;

namespace Gadec.Common.Tests._obsolete;
internal class DataRowExtensionsTests
{
    [Test]
    public void Test_SetString()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("First name");
        dataTable.Columns.Add("Last name");
        var dataRow = dataTable.NewRow();

        dataRow.SetString("First name", "Susan");

        Assert.That(dataRow["First name"], Is.EqualTo("Susan"));
    }

    [Test]
    public void Test_SetString_when_column_not_exists()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("First name");
        dataTable.Columns.Add("Last name");
        var dataRow = dataTable.NewRow();

        dataRow.SetString("Email", "susan@email.com");

        Assert.That(dataRow["Email"], Is.EqualTo("susan@email.com"));
    }

    [Test]
    public void Test_GetColumnNames()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("First name");
        dataTable.Columns.Add("Last name");
        var dataRow = dataTable.NewRow();

        var columnNames = dataRow.GetColumnNames();

        Assert.Multiple(() =>
        {
            Assert.That(columnNames[0], Is.EqualTo("First name"));
            Assert.That(columnNames[1], Is.EqualTo("Last name"));
        });
    }

    [Test]
    public void Test_GetString()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("First name");
        dataTable.Columns.Add("Last name");
        var dataRow = dataTable.NewRow();
        dataRow["First name"] = "Susan";

        var firstName = dataRow.GetString("First name");
        var lastName = dataRow.GetString("Last name");
        var email = dataRow.GetString("Email");

        Assert.Multiple(() =>
        {
            Assert.That(firstName, Is.EqualTo("Susan"));
            Assert.That(lastName, Is.EqualTo(string.Empty));
            Assert.That(email, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void Test_GetDouble()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("First name", typeof(string));
        dataTable.Columns.Add("Last name", typeof(string));
        dataTable.Columns.Add("Number string", typeof(string));
        dataTable.Columns.Add("Number double", typeof(double));
        var dataRow = dataTable.NewRow();
        dataRow["First name"] = "Susan";
        dataRow["Number string"] = "12,45";
        dataRow["Number double"] = 16.67;

        var hasNoNumber = dataRow.GetDouble("First name");
        var hasNoValue = dataRow.GetDouble("Last name");
        var numberFromString = dataRow.GetDouble("Number string");
        var numberFromDouble = dataRow.GetDouble("Number double");

        Assert.Multiple(() =>
        {
            Assert.That(hasNoNumber, Is.EqualTo(null));
            Assert.That(hasNoValue, Is.EqualTo(null));
            Assert.That(numberFromString, Is.EqualTo(12.45));
            Assert.That(numberFromDouble, Is.EqualTo(16.67));
        });
    }
}