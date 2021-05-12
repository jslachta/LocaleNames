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

            Assert.AreEqual("DE", localeNames.FindCountryCode("Germany"));

            Assert.AreEqual("CZ", localeNames.FindCountryCode("Czech Republic"));

            Assert.AreEqual("CZ", localeNames.FindCountryCode("Czechia"));

            Assert.AreEqual("GB", localeNames.FindCountryCode("United Kingdom"));
        }
    }
}
