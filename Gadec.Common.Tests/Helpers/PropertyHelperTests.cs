using Gadec.Common.Helpers;

namespace Gadec.Common.Tests.Helpers;
internal class PropertyHelperTests
{
    [Test]
    public void Test_Set_property()
    {
        var test = new TestModel { Name = "Initial", Email = "@" };

        PropertyHelper.Set(test, "Name", "New name");
        PropertyHelper.Set(test, "Email", "new.name@test.com");

        Assert.Multiple(() =>
        {
            Assert.That(test.Name, Is.EqualTo("New name"));
            Assert.That(test.Email, Is.EqualTo("new.name@test.com"));
        });
    }

    [Test]
    public void Test_Set_non_exist_property_should_throw_exception()
    {
        var test = new TestModel { Name = "Initial", Email = "@" };

        Assert.Throws<ArgumentException>(() => PropertyHelper.Set(test, "Value", "new.name@test.com"));
    }

    private class TestModel
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
