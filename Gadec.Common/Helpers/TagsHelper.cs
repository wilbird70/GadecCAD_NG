using Gadec.Common.Extensions;

namespace Gadec.Common.Helpers;
public class TagsHelper
{
    private readonly List<string> _tagCollection = [];

    public string GetUniqueTag(string tag)
    {
        var result = tag;
        while (_tagCollection.Contains(result))
        {
            result = result.AutoNumber();
        }
        _tagCollection.Add(result);
        return result;
    }
}
