using LocaleNames.Enumerations;
using LocaleNames.Infrastructure.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LocaleNames.Extensions
{
    /// <summary>
    /// Extensions method for dictionaries
    /// </summary>
    public static class DictionaryExtensions
    {
        // in the order the variants are returned in
        private static readonly AltVariant[] Variants =
        {
            AltVariant.Common, AltVariant.Alternative, AltVariant.Short, AltVariant.Long,
            AltVariant.Menu, AltVariant.Narrow, AltVariant.Formal,
        };

        private static readonly IReadOnlyDictionary<AltVariant, string> Empty =
            new ReadOnlyDictionary<AltVariant, string>(new Dictionary<AltVariant, string>());

        /// <summary>
        /// Finds all name variants of the country.
        /// </summary>
        /// <param name="sourceDictionary">The source dictionary</param>
        /// <param name="key">country code</param>
        /// <returns></returns>
        public static IReadOnlyDictionary<AltVariant, string> FindLocaleValues(
            this IDictionary<string, string> sourceDictionary,
            string key)
        {
            ExceptionGuard.ThrowIfNull(key);

            Dictionary<AltVariant, string> names = null;

            foreach (var variant in Variants)
            {
                if (sourceDictionary.TryGetValue(key + variant.GetLocalePostfix(), out var value))
                {
                    names ??= new Dictionary<AltVariant, string>();
                    names.Add(variant, value);
                }
            }

            return names == null ? Empty : new ReadOnlyDictionary<AltVariant, string>(names);
        }
    }
}
