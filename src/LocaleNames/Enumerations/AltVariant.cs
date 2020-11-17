using System;
using System.Collections.Generic;
using System.Text;

namespace LocaleNames.Enumerations
{
    /// <summary>
    /// Provides variants for name.
    /// </summary>
    [Flags]
    public enum AltVariant
    {
        /// <summary>
        /// The common name variant.
        /// </summary>
        Common = 0,

        /// <summary>
        /// The alternative name variant.
        /// </summary>
        Alternative = 1,

        /// <summary>
        /// The short name variant.
        /// </summary>
        Short = 2,

        /// <summary>
        /// The long name variant.
        /// </summary>
        Long = 4,

        /// <summary>
        /// The menu name variant.
        /// </summary>
        Menu = 8
    }

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
                case AltVariant.Common:
                    return string.Empty;
                case AltVariant.Alternative:
                    return "-alt-variant";
                case AltVariant.Short:
                    return "-alt-short";
                case AltVariant.Long:
                    return "-alt-long";
                case AltVariant.Menu:
                    return "-alt-menu";
                default:
                    return string.Empty;
            }
        }
    }
}
