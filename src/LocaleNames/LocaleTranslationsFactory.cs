using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LocaleNames
{
    /// <summary>
    /// Locale Names.
    /// </summary>
    public static class LocaleTranslationsFactory
    {
        private static ConcurrentDictionary<CultureInfo, LocaleTranslations> CachedLocaleNames
            = new ConcurrentDictionary<CultureInfo, LocaleTranslations>();

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public static void ClearCache()
            => CachedLocaleNames.Clear();

        /// <summary>
        /// Creates instance of <see cref="LocaleTranslations"/> for given language code.
        /// </summary>
        /// <param name="languageCode">The language code.</param>
        /// <returns></returns>
        public static LocaleTranslations ForLanguageCode(string languageCode)
        {
            CultureInfo cultureInfo = null;

            try
            {
                cultureInfo = CultureInfo.GetCultureInfo(languageCode);
            }
            catch (Exception)
            {
                cultureInfo = CultureInfo.InvariantCulture;
            }

            return ForCultureInfo(cultureInfo);
        }

        /// <summary>
        /// Creates instance of <see cref="LocaleTranslations"/> for current culture.
        /// </summary>
        /// <returns></returns>
        public static LocaleTranslations ForCurrentCulture()
        {
            var currentCulture = CultureInfo.CurrentCulture;

            return ForCultureInfo(currentCulture);
        }

        /// <summary>
        /// Creates instance of <see cref="LocaleTranslations"/> for given culture.
        /// </summary>
        /// <param name="cultureInfo">The culture information.</param>
        /// <returns></returns>
        public static LocaleTranslations ForCultureInfo(CultureInfo cultureInfo)
        {
            if (CachedLocaleNames.ContainsKey(cultureInfo))
            {
                var cachedlocalenames = CachedLocaleNames[cultureInfo];
                cachedlocalenames.IsFromCache = true;

                return cachedlocalenames;
            }
            else
            {
                var localeNames = new LocaleTranslations(cultureInfo);
                CachedLocaleNames.TryAdd(cultureInfo, localeNames);

                return localeNames;
            }
        }
    }
}
