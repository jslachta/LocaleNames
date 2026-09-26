using LocaleNames.Enumerations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace LocaleNames.Test
{
    /// <summary>
    /// Currency tests.
    /// </summary>
    [TestClass]
    public class FindCurrencyTests
    {
        /// <summary>
        /// Finds the currency name by code.
        /// </summary>
        [TestMethod]
        public void Find_currency_name_by_code()
        {
            var localeNames = LocaleTranslationsFactory.ForLanguageCode("cs-CZ");

            Assert.IsFalse(localeNames.AreCurrencyTranslationsEmpty);
            Assert.AreEqual("česká koruna", localeNames.FindCurrencyName("CZK"));
            Assert.AreEqual("česká koruna", localeNames.FindCurrencyName("czk"));
            Assert.AreEqual("euro", localeNames.FindCurrencyName("EUR"));
            Assert.IsNull(localeNames.FindCurrencyName("unknown code"));
            Assert.IsNull(localeNames.FindCurrencyName(null));

            Assert.AreEqual("Czech Koruna", LocaleTranslationsFactory.ForLanguageCode("en-US").FindCurrencyName("CZK"));
        }

        /// <summary>
        /// Finds the plural forms of the currency name.
        /// </summary>
        [TestMethod]
        public void Find_currency_plural_names_by_code()
        {
            var localeNames = LocaleTranslationsFactory.ForLanguageCode("cs-CZ");
            var result = localeNames.FindCurrencyPluralNames("CZK");

            Assert.HasCount(4, result);
            Assert.AreEqual("česká koruna", result[PluralCategory.One]);
            Assert.AreEqual("české koruny", result[PluralCategory.Few]);
            Assert.AreEqual("české koruny", result[PluralCategory.Many]);
            Assert.AreEqual("českých korun", result[PluralCategory.Other]);

            Assert.AreEqual("české koruny", localeNames.FindCurrencyName("CZK", PluralCategory.Few));
            Assert.AreEqual(null, localeNames.FindCurrencyName("CZK", PluralCategory.Two));
        }

        /// <summary>
        /// Finds the currency symbols by code.
        /// </summary>
        [TestMethod]
        public void Find_currency_symbol_by_code()
        {
            var localeNames = LocaleTranslationsFactory.ForLanguageCode("en-US");

            Assert.AreEqual("CZK", localeNames.FindCurrencySymbol("CZK"));
            Assert.AreEqual("Kč", localeNames.FindCurrencySymbol("CZK", AltVariant.Narrow));
            Assert.AreEqual("$", localeNames.FindCurrencySymbol("USD"));
            Assert.IsNull(localeNames.FindCurrencySymbol("unknown code"));

            var symbols = LocaleTranslationsFactory.ForLanguageCode("cs-CZ").FindCurrencySymbols("CZK");

            Assert.HasCount(2, symbols);
            Assert.AreEqual("Kč", symbols[AltVariant.Common]);
            Assert.AreEqual("Kč", symbols[AltVariant.Narrow]);
        }

        /// <summary>
        /// Finds the currency code by name.
        /// </summary>
        [TestMethod]
        public void Find_currency_code_by_name()
        {
            var localeNames = LocaleTranslationsFactory.ForLanguageCode("cs-CZ");

            Assert.AreEqual("CZK", localeNames.FindCurrencyCode("česká koruna"));
            Assert.IsNull(localeNames.FindCurrencyCode("českých korun"));
            Assert.IsNull(localeNames.FindCurrencyCode("unknown currency"));
        }

        /// <summary>
        /// Gets all currency codes.
        /// </summary>
        [TestMethod]
        public void Get_all_currency_codes()
        {
            var codes = LocaleTranslationsFactory.ForLanguageCode("en-US").GetAllCurrencyCodes();

            Assert.Contains("CZK", codes);
            Assert.Contains("EUR", codes);
            Assert.IsTrue(codes.All(i => i.Length == 3), "All currency codes are ISO 4217 codes.");
            Assert.AreEqual(codes.Count, codes.Distinct().Count());
        }
    }
}
