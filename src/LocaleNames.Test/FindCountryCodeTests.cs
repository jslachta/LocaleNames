﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}
