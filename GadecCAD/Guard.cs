using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace GadecCAD;

public static class Guard
{
    [return: NotNull]
    public static T ForNull<T>(T instance, [CallerArgumentExpression(nameof(instance))] string? parameterName = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(instance, parameterName);
        return instance;
    }

    public static void ForNull<T>([NotNull] T? instance, [CallerArgumentExpression(nameof(instance))] string? parameterName = null)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(instance, parameterName);
    }
}
