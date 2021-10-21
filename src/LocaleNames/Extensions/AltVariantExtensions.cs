using LocaleNames.Enumerations;
using System;

namespace LocaleNames.Extensions
{
    /// <summary>
    /// Extension methods for the enum <see cref="AltVariant"/>.
    /// </summary>
    internal static class AltVariantExtensions
    {
        /// <summary>
        /// Gets the locale postfix.
        /// </summary>
        /// <param name="variant">The variant.</param>
        /// <returns></returns>
        public static string GetLocalePostfix(this AltVariant variant)
        {
            switch (variant)
            {
                case AltVariant.Alternative:
                    return "-alt-variant";
                case AltVariant.Short:
                    return "-alt-short";
                case AltVariant.Long:
                    return "-alt-long";
                case AltVariant.Menu:
                    return "-alt-menu";
                default:
                case AltVariant.Common:
                    return string.Empty;
            }
        }
    }
}
