using System.Reflection;

namespace Gadec.Common.Helpers;
public static class PropertyHelper
{
    public static void Set(object model, string propertyName, object value)
    {
        PropertyInfo? propertyInfo = model.GetType().GetProperty(propertyName);
        if (propertyInfo?.CanWrite == true)
        {
            propertyInfo.SetValue(model, Convert.ChangeType(value, propertyInfo.PropertyType));
        }
        else
        {
            Console.WriteLine($"Cannot set property '{propertyName}'.");
        }
    }
}
