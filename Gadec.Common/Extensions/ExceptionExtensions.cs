namespace Gadec.Common.Extensions;
public static class ExceptionExtensions
{
    public static void Rethrow(this Exception eException)
    {
        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(eException).Throw();
    }

    public static void AddData(this Exception eException, string text)
    {
        var key = "1: ";
        while (eException.Data.Contains(key))
        {
            key = key.AutoNumber();
        }
        eException.Data.Add(key, text);
    }
}
