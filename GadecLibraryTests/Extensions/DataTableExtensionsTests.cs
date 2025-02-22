using GadecLibrary._obsolete;
using System.Data;

namespace GadecLibraryTests.Extensions;
internal class DataTableExtensionsTests
{
    [Test]
    public void Test_GetColumnNames()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("First name");
        dataTable.Columns.Add("Last name");

        var columnNames = dataTable.GetColumnNames();

        Assert.Multiple(() =>
        {
            Assert.That(columnNames[0], Is.EqualTo("First name"));
            Assert.That(columnNames[1], Is.EqualTo("Last name"));
        });
    }

    [Test]
    public void Test_GetTable()
    {
        var dataTable1 = new DataTable("First table");
        var dataTable2 = new DataTable("Second table");
        _ = new DataTable("Third table");
        var dataSet = new DataSet();
        dataSet.Tables.Add(dataTable1);
        dataSet.Tables.Add(dataTable2);

        var result2 = dataTable1.GetTable("Second table");
        var result3 = dataTable1.GetTable("Third table");
        var result4 = dataTable1.GetTable("Fourth table");
        var result5 = dataTable1.GetTable("");

        Assert.That(result2, Is.Not.Null);
        Assert.That(result3, Is.Not.Null);
        Assert.That(result4, Is.Not.Null);
        Assert.That(result5, Is.Null);
        Assert.That(result2.TableName, Is.EqualTo("Second table"));
        Assert.That(result3.TableName, Is.EqualTo("Third table"));
        Assert.That(result4.TableName, Is.EqualTo("Fourth table"));
    }

    [TestCase("", 0)]
    [TestCase("  ", 0)]
    [TestCase("First name;  ", 1)]
    [TestCase("First name;Last name", 2)]
    public void Test_AssignPrimaryKey(string primaryKeyString, int expectedNumberOfPrimaryKeys)
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("First name");
        dataTable.Columns.Add("Last name");
        dataTable.Columns.Add("Email");

        var row1 = dataTable.NewRow();
        row1["First name"] = "Will";
        row1["Last name"] = "Smith";
        var row2 = dataTable.NewRow();
        row2["First name"] = "George";
        row2["Last name"] = "Clooney";
        dataTable.Rows.Add(row1);
        dataTable.Rows.Add(row2);

        dataTable.AssignPrimaryKey(primaryKeyString);

        Assert.That(dataTable.PrimaryKey, Has.Length.EqualTo(expectedNumberOfPrimaryKeys));
    }

    [TestCase("Email")]
    [TestCase("First name;Email")]
    public void Test_AssignPrimaryKey_when_a_primary_key_does_not_exists_should_throw_exception(string primaryKeyString)
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("First name");
        dataTable.Columns.Add("Last name");

        Assert.Throws<ColumnNotFoundException>(() => dataTable.AssignPrimaryKey(primaryKeyString));
    }

    [Test]
    public void Test_AssignPrimaryKey_when_column_has_no_unique_values_should_throw_exception()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("First name");
        dataTable.Columns.Add("Last name");

        var row1 = dataTable.NewRow();
        row1["First name"] = "Will";
        row1["Last name"] = "Smith";
        var row2 = dataTable.NewRow();
        row2["First name"] = "Will";
        row2["Last name"] = "Clooney";
        dataTable.Rows.Add(row1);
        dataTable.Rows.Add(row2);

        Assert.Throws<ArgumentException>(() => dataTable.AssignPrimaryKey("First name"));
    }
}
