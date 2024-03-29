﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
