using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LocaleNames.Test
{
    /// <summary>
    /// FindCountryName tests.
    /// </summary>
    [TestClass]
    public class FindCountryNameTests
    {
        /// <summary>
        /// Finds the country name by code.
        /// </summary>
        [TestMethod]
        public void Find_country_name_by_code()
        {
            var localeNames = LocaleTranslationsFactory.ForLanguageCode("en-US");

            Assert.AreEqual("Germany", localeNames.FindCountryName("DE"));
            Assert.AreEqual("Czechia", localeNames.FindCountryName("CZ"));
            Assert.AreEqual("United Kingdom", localeNames.FindCountryName("GB"));
            Assert.AreEqual(null, localeNames.FindCountryName("unknown code"));
        }

        /// <summary>
        /// Fallback uses the intermediate script locale (sr-Latn-RS -> sr_Latn, not sr).
        /// </summary>
        [TestMethod]
        public void Find_country_name_falls_back_to_script_locale()
        {
            // values taken from language.sr_Latn.territories and language.zh_Hant.territories resources
            Assert.AreEqual("Nemačka", LocaleTranslationsFactory.ForLanguageCode("sr-Latn-RS").FindCountryName("DE"));
            Assert.AreEqual("德國", LocaleTranslationsFactory.ForLanguageCode("zh-TW").FindCountryName("DE"));
        }

        /// <summary>
        /// Fallback through the culture chain does not change results of other cultures.
        /// </summary>
        [TestMethod]
        public void Find_country_name_fallback_regression()
        {
            // values were verified against the behavior before the culture chain fallback was introduced
            Assert.AreEqual("Německo", LocaleTranslationsFactory.ForLanguageCode("cs-CZ").FindCountryName("DE"));
            Assert.AreEqual("Germany", LocaleTranslationsFactory.ForLanguageCode("en-US").FindCountryName("DE"));
            Assert.AreEqual("Alemanha", LocaleTranslationsFactory.ForLanguageCode("pt-PT").FindCountryName("DE"));
            Assert.AreEqual("Deutschland", LocaleTranslationsFactory.ForLanguageCode("de-AT").FindCountryName("DE"));
        }

        /// <summary>
        /// Finds all variants of country name by code.
        /// </summary>
        [TestMethod]
        public void Find_all_variants_of_country_name_by_code()
        {
            var localeNames = LocaleTranslationsFactory.ForLanguageCode("en-US");
            var result = localeNames.FindCountryNames("CZ");

            Assert.IsTrue(result.Count == 2, "CZ country name has only two variants");
            Assert.IsTrue(result[Enumerations.AltVariant.Common] == "Czechia");
            Assert.IsTrue(result[Enumerations.AltVariant.Alternative] == "Czech Republic");
        }
    }
}
