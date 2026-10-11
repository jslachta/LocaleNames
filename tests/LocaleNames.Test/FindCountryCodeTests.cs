using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using LocaleNames;
using System.Linq;
using LocaleNames.Extensions;

namespace LocaleNames.Test
{
    /// <summary>
    /// FindCountryCode tests.
    /// </summary>
    [TestClass]
    public class FindCountryCodeTests
    {
        /// <summary>
        /// All country codes should not provide variants and continents.
        /// </summary>
        [TestMethod]
        public void All_Country_Codes_Should_Not_Provide_Variants_And_Continents()
        {
            var localeNames = LocaleTranslationsFactory.ForLanguageCode("en-US");

            var countryCodes = localeNames.GetAllCountryCodes();

            Assert.IsTrue(countryCodes.Any(), "Testing on empty collection does not make sense.");

            /*
             * language code should not provide language variants, only the unique list of language codes
             */

            Assert.IsFalse(countryCodes.Any(i => i.IsCountryCodeContinent()));

            Assert.IsFalse(countryCodes.Any(i => i.Contains("-alt-variant")));
            Assert.IsFalse(countryCodes.Any(i => i.Contains("-alt-long")));
            Assert.IsFalse(countryCodes.Any(i => i.Contains("-alt-menu")));
            Assert.IsFalse(countryCodes.Any(i => i.Contains("-alt-short")));
        }

        /// <summary>
        /// Finds the country code by country name.
        /// </summary>
        [TestMethod]
        public void Find_country_code_by_name()
        {
            var localeNames = LocaleTranslationsFactory.ForLanguageCode("en-US");

            Assert.AreEqual("DE", localeNames.FindCountryCode("Germany"));

            Assert.AreEqual("CZ", localeNames.FindCountryCode("Czech Republic"));

            Assert.AreEqual("CZ", localeNames.FindCountryCode("Czechia"));

            Assert.AreEqual("GB", localeNames.FindCountryCode("United Kingdom"));
        }

        /// <summary>
        /// Unknown country name returns null instead of throwing.
        /// </summary>
        [TestMethod]
        public void Find_country_code_by_unknown_name_returns_null()
        {
            var localeNames = LocaleTranslationsFactory.ForLanguageCode("en-US");

            Assert.IsNull(localeNames.FindCountryCode("NonExistingCountryName"));
            Assert.IsNull(localeNames.FindCountryCode(null));
        }

        /// <summary>
        /// The names are compared ordinally, but a decomposed (NFD) name matches the composed (NFC) one.
        /// </summary>
        [TestMethod]
        public void Find_country_code_compares_names_ordinally()
        {
            var localeNames = LocaleTranslationsFactory.ForLanguageCode("cs-CZ");

            Assert.AreEqual("DE", localeNames.FindCountryCode("Německo"));
            Assert.AreEqual("DE", localeNames.FindCountryCode("Německo".Normalize(System.Text.NormalizationForm.FormD)));
            Assert.IsNull(localeNames.FindCountryCode("německo"));
        }

        /// <summary>
        /// The invisible characters (soft hyphen, zero width joiners, direction marks) are ignored, as the data differs in them.
        /// </summary>
        [TestMethod]
        public void Find_code_ignores_invisible_characters()
        {
            var localeNames = LocaleTranslationsFactory.ForLanguageCode("en-US");

            Assert.AreEqual("DE", localeNames.FindCountryCode("Ger­many"));
            Assert.AreEqual("DE", localeNames.FindCountryCode("Germany‏"));
            Assert.AreEqual("CZK", localeNames.FindCurrencyCode("Czech‌ Koruna"));
            Assert.AreEqual("de", localeNames.FindLanguageCode("‎German"));
        }

        /// <summary>
        /// The list of the codes is built once.
        /// </summary>
        [TestMethod]
        public void All_country_codes_are_cached()
        {
            var localeNames = LocaleTranslationsFactory.ForLanguageCode("en-US");

            Assert.AreSame(localeNames.GetAllCountryCodes(), localeNames.GetAllCountryCodes());
        }
    }
}
