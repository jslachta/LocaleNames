using LocaleNames.Enumerations;
using LocaleNames.Extensions;
using LocaleNames.Model;
using LocaleNames.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace LocaleNames
{
    /// <summary>
    /// 
    /// </summary>
    public class LocaleNames
    {
        #region PROPERTIES

        /// <summary>
        /// Gets a value indicating whether are language translations empty.
        /// </summary>
        public bool AreLanguageTranslationsEmpty => LanguageNames.Value.Keys.Count <= 0;

        /// <summary>
        /// Gets a value indicating whether are countryname translations empty.
        /// </summary>
        public bool AreCountryNameTranslationsEmpty => CountryNames.Value.Keys.Count <= 0;

        /// <summary>
        /// Gets a value indicating whether this instance is from cache.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is from cache; otherwise, <c>false</c>.
        /// </value>
        public bool IsFromCache { get; internal set; } = false;

        /// <summary>
        /// Gets the culture information.
        /// </summary>
        /// <value>
        /// The culture information.
        /// </value>
        public CultureInfo CultureInfo { get; private set; }

        /// <summary>
        /// Gets the language names.
        /// </summary>
        /// <value>
        /// The language names.
        /// </value>
        private readonly Lazy<ReadOnlyDictionary<string, string>> LanguageNames;

        /// <summary>
        /// Gets the language names.
        /// </summary>
        /// <value>
        /// The language names.
        /// </value>
        private readonly Lazy<ReadOnlyDictionary<string, string>> CountryNames;

        #endregion PROPERTIES

        #region CONSTRUCTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="LocaleNames"/> class.
        /// </summary>
        /// <param name="culture">The culture.</param>
        internal LocaleNames(CultureInfo culture)
        {
            CultureInfo = culture;

            CountryNames = new Lazy<ReadOnlyDictionary<string, string>>(() => TryLoadDictionary(CultureInfo, "territories"), true);
            LanguageNames = new Lazy<ReadOnlyDictionary<string, string>>(() => TryLoadDictionary(CultureInfo, "languages"), true);
        }

        #endregion CONSTRUCTOR

        #region DICTIONARY INIT

        /// <summary>
        /// Tries the load dictionary.
        /// </summary>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="postfix">The postfix.</param>
        /// <returns></returns>
        private ReadOnlyDictionary<string, string> TryLoadDictionary(CultureInfo cultureInfo, string postfix)
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

            return new ReadOnlyDictionary<string, string>(resultDictionary);
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
                .FirstOrDefault(i => i.EndsWith(resourceName));

            if (targetManifestResourceName != null)
            {
                using (Stream resourceStream = this.GetType().Assembly.GetManifestResourceStream(targetManifestResourceName))
                {
                    using (StreamReader streamReader = new StreamReader(resourceStream))
                    {
                        var compressedResourceValue = streamReader.ReadToEnd();
                        var decompressedResourceValue = GzipUtils.Decompress(compressedResourceValue);

                        dict = JsonSerializer.Deserialize<ResourceLocale>(decompressedResourceValue).Values;

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
        /// Provides all language codes.
        /// </summary>
        public IReadOnlyCollection<string> GetAllLanguageCodes()
            => new ReadOnlyCollection<string>(LanguageNames.Value.Select(i => i.Key.StripLocaleVariants()).Distinct().ToList());

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

                languageNames = LanguageNames.Value.FindLocaleValues(ci.Name);

                if (!languageNames.Any())
                {
                    languageNames = LanguageNames.Value.FindLocaleValues(ci.TwoLetterISOLanguageName);
                }

                if (!languageNames.Any())
                {
                    languageNames = LanguageNames.Value.FindLocaleValues(ci.ThreeLetterISOLanguageName);
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
            var value = LanguageNames.Value.FirstOrDefault(i => string.Compare(i.Value, countryName) == 0);

            return value.Key;
        }

        #endregion FIND LANGUAGE NAMES/CODES

        #region FIND COUNTRY NAMES/CODES

        /// <summary>
        /// Provides all country codes.
        /// </summary>
        public IReadOnlyCollection<string> GetAllCountryCodes()
            => new ReadOnlyCollection<string>(
                CountryNames
                .Value
                .Where(i => !i.Key.IsCountryCodeContinent())
                .Select(i => i.Key.StripLocaleVariants())
                .Distinct().ToList());

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
            return CountryNames.Value.FindLocaleValues(countryCode);
        }

        /// <summary>
        /// Finds the country code.
        /// </summary>
        /// <param name="countryName">Name of the country.</param>
        /// <returns></returns>
        public string FindCountryCode(string countryName)
        {
            var value = CountryNames.Value.FirstOrDefault(i => string.Compare(i.Value, countryName) == 0);
            var result = value.Key;

            return result.StripLocaleVariants();
        }

        #endregion FIND COUNTRY NAMES/CODES
    }
}
