using System;
using System.Collections.Generic;
using System.Text;
using LocaleNames.Enumerations;

namespace LocaleNames.Extensions
{
    /// <summary>
    /// The string extensions.
    /// </summary>
    public static class StringExtensions
    {
        // listed explicitly: Enum.GetValues(Type) is not safe for native AOT
        private static readonly AltVariant[] AltVariants =
        {
            AltVariant.None, AltVariant.Common, AltVariant.Alternative, AltVariant.Short,
            AltVariant.Long, AltVariant.Menu, AltVariant.Narrow, AltVariant.Formal,
        };

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
            foreach (var suit in AltVariants)
            {
                if (!string.IsNullOrWhiteSpace(suit.GetLocalePostfix()) && inputText.Contains(suit.GetLocalePostfix()))
                {
                    inputText = inputText.Replace(suit.GetLocalePostfix(), "");
                }
            }

            return inputText;
        }
    }
}
