using System;
using System.Runtime.CompilerServices;

namespace LocaleNames.Infrastructure.Diagnostics
{
    /// <summary>
    /// Centralizes exception throwing for common argument validation scenarios used across the library.
    /// </summary>
    /// <remarks>
    /// Wraps the BCL guard methods that are not available on every target framework.
    /// </remarks>
    internal static class ExceptionGuard
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> when the <paramref name="argument"/> is null.
        /// </summary>
        /// <param name="argument">The argument to validate.</param>
        /// <param name="paramName">Name of the parameter that is validated, captured from the caller's expression.</param>
        internal static void ThrowIfNull(object argument, [CallerArgumentExpression("argument")] string paramName = null)
        {
#if NET6_0_OR_GREATER
            // Our own parameter is captured here as "argument", so the caller's name is passed explicitly.
#pragma warning disable S3236 // Caller information arguments should not be provided explicitly
            ArgumentNullException.ThrowIfNull(argument, paramName);
#pragma warning restore S3236
#else
            if (argument == null)
            {
                throw new ArgumentNullException(paramName);
            }
#endif
        }
    }
}


