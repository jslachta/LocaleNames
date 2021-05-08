using LocaleNames.Enumerations;
using LocaleNames.Extensions;
using LocaleNames.Utils;
using Newtonsoft.Json;
using PrepareLocaleData.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LocaleNames
{
    /// <summary>
    /// Locale Names.
    /// </summary>
    public class LocaleTranslations
    {
        #region STATIC PROPERTIES

        /// <summary>
        /// Gets the default culture information.
        /// </summary>
        /// <value>
        /// The default culture information.
        /// </value>
        public static CultureInfo DefaultCultureInfo
        {
            get
            {
                return defaultCultureInfo;
            }
            set
            {
                if (value != CultureInfo.InvariantCulture)
                {
                    defaultCultureInfo = value;
                }
                else
                {
                    defaultCultureInfo = new CultureInfo("en-US");
                }
            }
        }

        private static CultureInfo defaultCultureInfo = new CultureInfo("en-US");

        #endregion STATIC PROPERTIES

        #region CACHE

        private static Dictionary<CultureInfo, LocaleTranslations> CachedLocaleNames { get; set; }
            = new Dictionary<CultureInfo, LocaleTranslations>();

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public static void ClearCache()
        {
            lock (CachedLocaleNames)
            {
                CachedLocaleNames.Clear();
            }
        }

        #endregion CACHE

        #region FACTORY

        /// <summary>
        /// Creates instance of <see cref="LocaleTranslations"/> for given language code.
        /// </summary>
        /// <param name="languageCode">The language code.</param>
        /// <param name="fallbackCulture">The fallback cultureInfo</param>
        /// <returns></returns>
        public static LocaleTranslations ForLanguageCode(string languageCode, CultureInfo fallbackCulture = null)
        {
            CultureInfo cultureInfo = null;

            try
            {
                cultureInfo = CultureInfo.GetCultureInfo(languageCode);
            }
            catch (Exception e)
            {
                if (fallbackCulture == null && (e is ArgumentNullException || e is CultureNotFoundException))
                {
                    throw;
                }
                else
                {
                    cultureInfo = fallbackCulture;
                }
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
            if (cultureInfo == CultureInfo.InvariantCulture)
            {
                cultureInfo = DefaultCultureInfo;
            }

            lock (CachedLocaleNames)
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
                    CachedLocaleNames.Add(cultureInfo, localeNames);

                    return localeNames;
                }
            }
        }

        #endregion FACTORY

        #region PROPERTIES

        /// <summary>
        /// Gets a value indicating whether this instance is from cache.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is from cache; otherwise, <c>false</c>.
        /// </value>
        public bool IsFromCache { get; private set; } = false;

        /// <summary>
        /// Gets the culture information.
        /// </summary>
        /// <value>
        /// The culture information.
        /// </value>
        public CultureInfo CultureInfo { get; private set; }

        private IDictionary<string, string> languageNames;
        /// <summary>
        /// Gets the language names.
        /// </summary>
        /// <value>
        /// The language names.
        /// </value>
        private IDictionary<string, string> LanguageNames
        {
            get
            {
                if (languageNames == null)
                {
                    languageNames = TryLoadDictionary(CultureInfo, "languages");
                }

                return languageNames;
            }
        }

        private IDictionary<string, string> countryNames;
        /// <summary>
        /// Gets the country names.
        /// </summary>
        /// <value>
        /// The country names.
        /// </value>
        private IDictionary<string, string> CountryNames
        {
            get
            {
                if (countryNames == null)
                {
                    countryNames = TryLoadDictionary(CultureInfo, "territories");
                }

                return countryNames;
            }
        }

        #endregion PROPERTIES

        #region CONSTRUCTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="LocaleTranslations"/> class.
        /// </summary>
        /// <param name="culture">The culture.</param>
        LocaleTranslations(CultureInfo culture)
        {
            CultureInfo = culture;
        }

        #endregion CONSTRUCTOR

        #region DICTIONARY INIT

        /// <summary>
        /// Tries the load dictionary.
        /// </summary>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="postfix">The postfix.</param>
        /// <returns></returns>
        private IDictionary<string, string> TryLoadDictionary(CultureInfo cultureInfo, string postfix)
        {
            IDictionary<string, string> resultDictionary = null;

            var result = LoadDictionary(cultureInfo.Name, postfix, ref resultDictionary);

            if (!result)
            {
                result = LoadDictionary(cultureInfo.TwoLetterISOLanguageName, postfix, ref resultDictionary);
            }

            if (!result)
            {
                result = LoadDictionary(cultureInfo.ThreeLetterISOLanguageName, postfix, ref resultDictionary);
            }

            if (!result)
            {
                LoadDictionary($"{cultureInfo.TwoLetterISOLanguageName}_POSIX", postfix, ref resultDictionary);
            }

            return resultDictionary;
        }

        /// <summary>
        /// Loads the dictionary. If the dictionary is not found by its key and postfix, the empty dictionary is created.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="postfix">The postfix.</param>
        /// <param name="dict">The dictionary.</param>
        /// <returns></returns>
        private bool LoadDictionary(string key, string postfix, ref IDictionary<string, string> dict)
        {
            key = key.Replace("-", "_");
            bool isFound = false;

            string resourceName = $"language.{key}.{postfix}.json.gz";
            string targetManifestResourceName = this
                .GetType().Assembly
                .GetManifestResourceNames()
                .Where(i => i.EndsWith(resourceName)).FirstOrDefault();

            if (targetManifestResourceName != null)
            {
                using (Stream resourceStream = this.GetType().Assembly.GetManifestResourceStream(targetManifestResourceName))
                {
                    using (StreamReader streamReader = new StreamReader(resourceStream))
                    {
                        var compressedResourceValue = streamReader.ReadToEnd();
                        var decompressedResourceValue = GzipUtils.Decompress(compressedResourceValue);

                        dict = JsonConvert.DeserializeObject<ResourceLocale>(decompressedResourceValue).Values;

                        isFound = true;
                    }
                }
            }

            if (!isFound && dict == null)
            {
                dict = new Dictionary<string, string>();
            }

            return isFound;
        }

        #endregion DICTIONARY INIT

        #region FIND LANGUAGE NAMES/CODES

        /// <summary>
        /// Finds the name of the country.
        /// </summary>
        /// <param name="languageCode">The country code.</param>
        /// <param name="variant"></param>
        /// <returns></returns>
        public string FindLanguageName(string languageCode, AltVariant variant = AltVariant.Common)
        {
            var languageNames = FindLanguageNames(languageCode);

            if (languageNames.ContainsKey(variant))
            {
                return languageNames[variant];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Finds all name variants of the language.
        /// </summary>
        /// <param name="languageCode">language code</param>
        /// <returns></returns>
        public IReadOnlyDictionary<AltVariant, string> FindLanguageNames(string languageCode)
        {
            IReadOnlyDictionary<AltVariant, string> languageNames;

            try
            {
                CultureInfo ci = new CultureInfo(languageCode);

                languageNames = LanguageNames.FindLocaleValues(ci.Name);

                if (!languageNames.Any())
                {
                    languageNames = LanguageNames.FindLocaleValues(ci.TwoLetterISOLanguageName);
                }

                if (!languageNames.Any())
                {
                    languageNames = LanguageNames.FindLocaleValues(ci.ThreeLetterISOLanguageName);
                }
            }
            catch (CultureNotFoundException)
            {
                languageNames = new ReadOnlyDictionary<AltVariant, string>(new Dictionary<AltVariant, string>());
            }

            return languageNames;
        }

        /// <summary>
        /// Finds the language code.
        /// </summary>
        /// <param name="countryName">Name of the country.</param>
        /// <returns></returns>
        public string FindLanguageCode(string countryName)
        {
            var value = LanguageNames.FirstOrDefault(i => i.Value == countryName);

            return value.Key;
        }

        #endregion

        #region FIND COUNTRY NAMES/CODES

        /// <summary>
        /// Finds the name of the country.
        /// </summary>
        /// <param name="countryCode">The country code.</param>
        /// <param name="variant"></param>
        /// <returns></returns>
        public string FindCountryName(string countryCode, AltVariant variant = AltVariant.Common)
        {
            var countryNames = FindCountryNames(countryCode);

            if (countryNames.ContainsKey(variant))
            {
                return countryNames[variant];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Finds all name variants of the country.
        /// </summary>
        /// <param name="countryCode">country code</param>
        /// <returns></returns>
        public IReadOnlyDictionary<AltVariant, string> FindCountryNames(string countryCode)
        {
            return CountryNames.FindLocaleValues(countryCode);
        }

        /// <summary>
        /// Finds the country code.
        /// </summary>
        /// <param name="countryName">Name of the country.</param>
        /// <returns></returns>
        public string FindCountryCode(string countryName)
        {
            var value = CountryNames.FirstOrDefault(i => i.Value == countryName);
            var result = value.Key;

            return result.StripLocaleVariants();
        }

        #endregion FIND METHODS

    }
}
