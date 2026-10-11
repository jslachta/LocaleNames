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
                case AltVariant.Narrow:
                    return "-alt-narrow";
                case AltVariant.Formal:
                    return "-alt-formal";
                default:
                case AltVariant.Common:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Gets the locale postfix of a single variant.
        /// </summary>
        /// <param name="variant">The variant.</param>
        /// <param name="postfix">The postfix, empty for <see cref="AltVariant.Common"/>.</param>
        /// <returns><c>false</c> for <see cref="AltVariant.None"/> and values that are not a single variant.</returns>
        public static bool TryGetLocalePostfix(this AltVariant variant, out string postfix)
        {
            postfix = variant.GetLocalePostfix();

            return postfix.Length != 0 || variant == AltVariant.Common;
        }
    }
}
