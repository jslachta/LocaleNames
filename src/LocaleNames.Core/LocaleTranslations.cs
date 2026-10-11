using LocaleNames.Enumerations;
using LocaleNames.Extensions;
using LocaleNames.Utils;
using System;
using System.Collections.Generic;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LocaleNames
{
    /// <summary>
    /// Locale translations for a single language.
    /// </summary>
    public class LocaleTranslations
    {
        #region PROPERTIES

        // listed explicitly: Enum.GetValues(Type) is not safe for native AOT; with the postfix of the resource key
        private static readonly KeyValuePair<PluralCategory, string>[] PluralCategories =
        {
            new(PluralCategory.Zero, "-count-zero"), new(PluralCategory.One, "-count-one"), new(PluralCategory.Two, "-count-two"),
            new(PluralCategory.Few, "-count-few"), new(PluralCategory.Many, "-count-many"), new(PluralCategory.Other, "-count-other"),
        };

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

        // reverse lookups (name -> code), built on the first use
        private readonly Lazy<IReadOnlyDictionary<string, string>> LanguageCodes;
        private readonly Lazy<IReadOnlyDictionary<string, string>> CountryCodes;
        private readonly Lazy<IReadOnlyDictionary<string, string>> CurrencyCodes;

        private readonly Lazy<IReadOnlyCollection<string>> AllLanguageCodes;
        private readonly Lazy<IReadOnlyCollection<string>> AllCountryCodes;
        private readonly Lazy<IReadOnlyCollection<string>> AllCurrencyCodes;

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

            LanguageCodes = new Lazy<IReadOnlyDictionary<string, string>>(
                () => BuildCodeLookup(LanguageNames.Value, key => key), true);
            CountryCodes = new Lazy<IReadOnlyDictionary<string, string>>(
                () => BuildCodeLookup(CountryNames.Value, key => key.StripLocaleVariants()), true);
            CurrencyCodes = new Lazy<IReadOnlyDictionary<string, string>>(
                () => BuildCodeLookup(CurrencyNames.Value.Where(i => !i.Key.Contains('-')), key => key), true);

            AllLanguageCodes = new Lazy<IReadOnlyCollection<string>>(
                () => ToCollection(LanguageNames.Value.Select(i => i.Key.StripLocaleVariants())), true);
            AllCountryCodes = new Lazy<IReadOnlyCollection<string>>(
                () => ToCollection(CountryNames.Value.Where(i => !i.Key.IsCountryCodeContinent()).Select(i => i.Key.StripLocaleVariants())), true);
            AllCurrencyCodes = new Lazy<IReadOnlyCollection<string>>(
                () => ToCollection(CurrencyNames.Value.Select(i => i.Key.Split('-')[0])), true);
        }

        #endregion CONSTRUCTOR

        #region DICTIONARY INIT

        /// <summary>
        /// Tries the load dictionary.
        /// </summary>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="postfix">The postfix.</param>
        /// <returns></returns>
        private static ReadOnlyDictionary<string, string> TryLoadDictionary(CultureInfo cultureInfo, string postfix)
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
        private static bool LoadDictionary(string key, string postfix, ref IDictionary<string, string> dict)
        {
            key = key.Replace("-", "_");
            bool isFound = false;

            using (Stream resourceStream = LocaleResourceProvider.Open($"language.{key}.{postfix}.lnb"))
            {
                if (resourceStream != null)
                {
                    dict = LocaleResourceFormat.Deserialize(resourceStream);

                    isFound = true;
                }
            }

            if (!isFound && dict == null)
            {
                dict = new Dictionary<string, string>();
            }

            return isFound;
        }

        /// <summary>
        /// Builds the lookup of a code by its name. The first entry of a name wins, as the entries keep the order of the data.
        /// </summary>
        private static IReadOnlyDictionary<string, string> BuildCodeLookup(
            IEnumerable<KeyValuePair<string, string>> entries,
            Func<string, string> codeSelector)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var item in entries)
            {
                var name = Normalize(item.Value);

                if (!result.ContainsKey(name))
                {
                    result.Add(name, codeSelector(item.Key));
                }
            }

#if NET8_0_OR_GREATER
            return result.ToFrozenDictionary(StringComparer.Ordinal);
#else
            return result;
#endif
        }

        private static string FindCode(IReadOnlyDictionary<string, string> codes, string name)
        {
            if (name == null)
            {
                return null;
            }

            return codes.TryGetValue(Normalize(name), out var code) ? code : null;
        }

        // The names are compared ordinally, like the culture sensitive comparison they replace, canonically
        // equivalent forms (NFC, NFD) match each other and the invisible characters are ignored (the data differs
        // in them, e.g. in the zero width non-joiner, between the forms of the same name).
        private static string Normalize(string value)
        {
            if (!value.IsNormalized())
            {
                value = value.Normalize();
            }

            if (!value.Any(IsIgnored))
            {
                return value;
            }

            var result = new System.Text.StringBuilder(value.Length);

            foreach (var c in value.Where(c => !IsIgnored(c)))
            {
                result.Append(c);
            }

            return result.ToString();
        }

        // invisible formatting characters (soft hyphen, zero width joiners, direction marks, byte order mark, ...)
        private static bool IsIgnored(char c) => char.GetUnicodeCategory(c) == UnicodeCategory.Format;

        private static ReadOnlyCollection<string> ToCollection(IEnumerable<string> codes)
            => new ReadOnlyCollection<string>(codes.Distinct().ToList());

        private static string FindLocaleValue(ReadOnlyDictionary<string, string> values, string key, AltVariant variant)
            => key != null && variant.TryGetLocalePostfix(out var postfix) && values.TryGetValue(key + postfix, out var value)
                ? value
                : null;

        #endregion DICTIONARY INIT

        #region FIND LANGUAGE NAMES/CODES

        /// <summary>
        /// Provides all language codes.
        /// </summary>
        public IReadOnlyCollection<string> GetAllLanguageCodes()
            => AllLanguageCodes.Value;

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

                if (languageNames.Count == 0)
                {
                    languageNames = LanguageNames.Value.FindLocaleValues(ci.TwoLetterISOLanguageName);
                }

                if (languageNames.Count == 0)
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
            => FindCode(LanguageCodes.Value, countryName);

        #endregion FIND LANGUAGE NAMES/CODES

        #region FIND COUNTRY NAMES/CODES

        /// <summary>
        /// Provides all country codes.
        /// </summary>
        public IReadOnlyCollection<string> GetAllCountryCodes()
            => AllCountryCodes.Value;

        /// <summary>
        /// Finds the name of the country.
        /// </summary>
        /// <param name="countryCode">The country code.</param>
        /// <param name="variant"></param>
        /// <returns></returns>
        public string FindCountryName(string countryCode, AltVariant variant = AltVariant.Common)
            => FindLocaleValue(CountryNames.Value, countryCode, variant);

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
            => FindCode(CountryCodes.Value, countryName);

        #endregion FIND COUNTRY NAMES/CODES

        #region FIND CURRENCY NAMES/SYMBOLS/CODES

        /// <summary>
        /// Provides all currency codes (ISO 4217).
        /// </summary>
        public IReadOnlyCollection<string> GetAllCurrencyCodes()
            => AllCurrencyCodes.Value;

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

            foreach (var category in PluralCategories)
            {
                if (CurrencyNames.Value.TryGetValue(code + category.Value, out var name))
                {
                    names.Add(category.Key, name);
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
            => FindLocaleValue(CurrencyNames.Value, $"{NormalizeCurrencyCode(currencyCode)}-symbol", variant);

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
            => FindCode(CurrencyCodes.Value, currencyName);

        private static string NormalizeCurrencyCode(string currencyCode)
            => currencyCode?.ToUpperInvariant() ?? string.Empty;

        #endregion FIND CURRENCY NAMES/SYMBOLS/CODES
    }
}
