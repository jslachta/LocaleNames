using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using LocaleNames;

namespace LocaleNames.Test
{
    [TestClass]
    public class FindCountryCodeTests
    {
        [TestMethod]
        public void LocaleNames_Find_country_code_by_name()
        {
            var localeNames = LocaleTranslations.ForLanguageCode("en-US");

            Assert.AreEqual(localeNames.FindCountryCode("Germany"), "DE");

            Assert.AreEqual(localeNames.FindCountryCode("Czech Republic"), "CZ");

            Assert.AreEqual(localeNames.FindCountryCode("Czechia"), "CZ");

            Assert.AreEqual(localeNames.FindCountryCode("United Kingdom"), "GB");
        }
    }
}
