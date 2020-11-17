using LocaleNames.Enumerations;
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
            Dictionary<AltVariant, string> names = new Dictionary<AltVariant, string>();

            if (sourceDictionary.ContainsKey(key))
            {
                names.Add(AltVariant.Common, sourceDictionary[key]);
            }

            if (sourceDictionary.ContainsKey($"{key}-alt-variant"))
            {
                names.Add(AltVariant.Alternative, sourceDictionary[$"{key}-alt-variant"]);
            }

            if (sourceDictionary.ContainsKey($"{key}-alt-short"))
            {
                names.Add(AltVariant.Short, sourceDictionary[$"{key}-alt-short"]);
            }

            if (sourceDictionary.ContainsKey($"{key}-alt-long"))
            {
                names.Add(AltVariant.Long, sourceDictionary[$"{key}-alt-long"]);
            }

            if (sourceDictionary.ContainsKey($"{key}-alt-menu"))
            {
                names.Add(AltVariant.Menu, sourceDictionary[$"{key}-alt-menu"]);
            }

            return new ReadOnlyDictionary<AltVariant, string>(names);
        }
    }
}
