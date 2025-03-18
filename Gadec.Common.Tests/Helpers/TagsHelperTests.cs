using Gadec.Common.Helpers;

namespace Gadec.Common.Tests.Helpers;
internal class TagsHelperTests
{
    [Test]
    public void Test_TagsHelper()
    {
        var tagsHelper = new TagsHelper();
        const string tag = "TAG";

        var firstTag = tagsHelper.GetUniqueTag(tag);
        var secondTag = tagsHelper.GetUniqueTag(tag);
        var thirdTag = tagsHelper.GetUniqueTag(tag);
        var fourthTag = tagsHelper.GetUniqueTag("Other TAG");
        var fifthTag = tagsHelper.GetUniqueTag(tag);

        Assert.Multiple(() =>
        {
            Assert.That(firstTag, Is.EqualTo("TAG"));
            Assert.That(secondTag, Is.EqualTo("TAG1"));
            Assert.That(thirdTag, Is.EqualTo("TAG2"));
            Assert.That(fourthTag, Is.EqualTo("Other TAG"));
            Assert.That(fifthTag, Is.EqualTo("TAG3"));
        });
    }
}
