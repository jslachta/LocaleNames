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
    /// Locale translations for a single language.
    /// </summary>
    public class LocaleTranslations
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
        /// Gets a value indicating whether are currency translations empty.
        /// </summary>
        public bool AreCurrencyTranslationsEmpty => CurrencyNames.Value.Keys.Count <= 0;

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

        /// <summary>
        /// Gets the currency names and symbols.
        /// </summary>
        /// <value>
        /// The currency names and symbols.
        /// </value>
        private readonly Lazy<ReadOnlyDictionary<string, string>> CurrencyNames;

        #endregion PROPERTIES

        #region CONSTRUCTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="LocaleTranslations"/> class.
        /// </summary>
        /// <param name="culture">The culture.</param>
        internal LocaleTranslations(CultureInfo culture)
        {
            CultureInfo = culture;

            CountryNames = new Lazy<ReadOnlyDictionary<string, string>>(() => TryLoadDictionary(CultureInfo, "territories"), true);
            LanguageNames = new Lazy<ReadOnlyDictionary<string, string>>(() => TryLoadDictionary(CultureInfo, "languages"), true);
            CurrencyNames = new Lazy<ReadOnlyDictionary<string, string>>(() => TryLoadDictionary(CultureInfo, "currencies"), true);
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

            var result = false;

            // walk the culture chain (e.g. sr-Latn-RS -> sr-Latn -> sr) so that intermediate script locales are honored
            for (var culture = cultureInfo; !result && !string.IsNullOrEmpty(culture.Name); culture = culture.Parent)
            {
                result = LoadDictionary(culture.Name, postfix, ref resultDictionary);
            }

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

            using (Stream resourceStream = LocaleResourceProvider.Open($"language.{key}.{postfix}.json.gz"))
            {
                if (resourceStream != null)
                {
                    using (StreamReader streamReader = new(resourceStream))
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
            => FindLanguageNames(languageCode).FirstOrDefault(i => i.Key == variant).Value;

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
                CultureInfo ci = new(languageCode);

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
            => LanguageNames.Value.FirstOrDefault(i => string.Compare(i.Value, countryName) == 0).Key;

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
            => FindCountryNames(countryCode).FirstOrDefault(i => i.Key == variant).Value;

        /// <summary>
        /// Finds all name variants of the country.
        /// </summary>
        /// <param name="countryCode">country code</param>
        /// <returns></returns>
        public IReadOnlyDictionary<AltVariant, string> FindCountryNames(string countryCode)
            => CountryNames.Value.FindLocaleValues(countryCode);

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

        #region FIND CURRENCY NAMES/SYMBOLS/CODES

        /// <summary>
        /// Provides all currency codes (ISO 4217).
        /// </summary>
        public IReadOnlyCollection<string> GetAllCurrencyCodes()
            => new ReadOnlyCollection<string>(
                CurrencyNames
                .Value
                .Select(i => i.Key.Split('-')[0])
                .Distinct().ToList());

        /// <summary>
        /// Finds the name of the currency.
        /// </summary>
        /// <param name="currencyCode">The currency code (ISO 4217).</param>
        /// <returns></returns>
        public string FindCurrencyName(string currencyCode)
            => CurrencyNames.Value.TryGetValue(NormalizeCurrencyCode(currencyCode), out var name) ? name : null;

        /// <summary>
        /// Finds the name of the currency for the given plural category (e.g. "české koruny" for <see cref="PluralCategory.Few"/>).
        /// </summary>
        /// <param name="currencyCode">The currency code (ISO 4217).</param>
        /// <param name="pluralCategory">The plural category.</param>
        /// <returns></returns>
        public string FindCurrencyName(string currencyCode, PluralCategory pluralCategory)
            => FindCurrencyPluralNames(currencyCode).TryGetValue(pluralCategory, out var name) ? name : null;

        /// <summary>
        /// Finds all plural forms of the currency name.
        /// </summary>
        /// <param name="currencyCode">The currency code (ISO 4217).</param>
        /// <returns></returns>
        public IReadOnlyDictionary<PluralCategory, string> FindCurrencyPluralNames(string currencyCode)
        {
            var code = NormalizeCurrencyCode(currencyCode);
            var names = new Dictionary<PluralCategory, string>();

            foreach (PluralCategory category in Enum.GetValues(typeof(PluralCategory)).Cast<PluralCategory>())
            {
                if (CurrencyNames.Value.TryGetValue($"{code}-count-{category.ToString().ToLowerInvariant()}", out var name))
                {
                    names.Add(category, name);
                }
            }

            return new ReadOnlyDictionary<PluralCategory, string>(names);
        }

        /// <summary>
        /// Finds the symbol of the currency.
        /// </summary>
        /// <param name="currencyCode">The currency code (ISO 4217).</param>
        /// <param name="variant">The symbol variant (<see cref="AltVariant.Common"/>, <see cref="AltVariant.Narrow"/>, <see cref="AltVariant.Alternative"/> or <see cref="AltVariant.Formal"/>).</param>
        /// <returns></returns>
        public string FindCurrencySymbol(string currencyCode, AltVariant variant = AltVariant.Common)
            => FindCurrencySymbols(currencyCode).FirstOrDefault(i => i.Key == variant).Value;

        /// <summary>
        /// Finds all variants of the currency symbol.
        /// </summary>
        /// <param name="currencyCode">The currency code (ISO 4217).</param>
        /// <returns></returns>
        public IReadOnlyDictionary<AltVariant, string> FindCurrencySymbols(string currencyCode)
            => CurrencyNames.Value.FindLocaleValues($"{NormalizeCurrencyCode(currencyCode)}-symbol");

        /// <summary>
        /// Finds the currency code.
        /// </summary>
        /// <param name="currencyName">Name of the currency.</param>
        /// <returns></returns>
        public string FindCurrencyCode(string currencyName)
            => CurrencyNames.Value
                .FirstOrDefault(i => !i.Key.Contains('-') && string.Compare(i.Value, currencyName) == 0)
                .Key;

        private static string NormalizeCurrencyCode(string currencyCode)
            => currencyCode?.ToUpperInvariant() ?? string.Empty;

        #endregion FIND CURRENCY NAMES/SYMBOLS/CODES
    }
}
