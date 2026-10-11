using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LocaleNames.Enumerations;

namespace LocaleNames.Extensions
{
    /// <summary>
    /// The string extensions.
    /// </summary>
    public static class StringExtensions
    {
        // the common part of all the variant postfixes ("-alt-short", ...)
        private const string AltVariantPrefix = "-alt-";

        // listed explicitly: Enum.GetValues(Type) is not safe for native AOT
        private static readonly string[] AltVariantPostfixes = new[]
            {
                AltVariant.Alternative, AltVariant.Short, AltVariant.Long,
                AltVariant.Menu, AltVariant.Narrow, AltVariant.Formal,
            }
            .Select(variant => variant.GetLocalePostfix())
            .ToArray();

        /// <summary>
        /// Determines whether the provided country code is continent.
        /// </summary>
        /// <param name="countryCode">The country code</param>
        /// <returns>Returns true if countryCode is continent, otherwise false.</returns>
        public static bool IsCountryCodeContinent(this string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode)) return false;

            return int.TryParse(countryCode, out _);
        }

        /// <summary>
        /// Strips the locale variants from the given string.
        /// </summary>
        /// <param name="inputText">The text.</param>
        /// <returns>The stripped locale variant</returns>
        public static string StripLocaleVariants(this string inputText)
        {
            // most of the keys have no variant
            if (inputText.IndexOf(AltVariantPrefix, StringComparison.Ordinal) < 0)
            {
                return inputText;
            }

            foreach (var postfix in AltVariantPostfixes)
            {
                inputText = inputText.Replace(postfix, "");
            }

            return inputText;
        }
    }
}
