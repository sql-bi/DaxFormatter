#if !NET
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

// The guard methods of .NET, for the target that does not have them: the shared code calls them by their .NET names.
internal static class ArgumentGuards
{
    extension(ArgumentNullException)
    {
        public static void ThrowIfNull(
            [NotNull] object? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }

    extension(ArgumentOutOfRangeException)
    {
        public static void ThrowIfNegative(
            int value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "The value must not be negative.");
            }
        }
    }
}
#endif
