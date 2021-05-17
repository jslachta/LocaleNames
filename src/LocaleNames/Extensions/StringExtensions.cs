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
        /// <summary>
        /// Strips the locale variants from the given string.
        /// </summary>
        /// <param name="inputText">The text.</param>
        /// <returns></returns>
        public static string StripLocaleVariants(this string inputText)
        {
            foreach (AltVariant suit in (AltVariant[])Enum.GetValues(typeof(AltVariant)))
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
