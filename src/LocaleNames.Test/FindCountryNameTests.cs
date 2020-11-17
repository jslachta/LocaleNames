using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using LocaleNames;

namespace LocaleNames.Test
{
    [TestClass]
    public class FindCountryNameTests
    {
        [TestMethod]
        public void Find_country_name_by_code()
        {
            var localeNames = LocaleTranslations.ForLanguageCode("en-US");

            Assert.AreEqual(localeNames.FindCountryName("DE"), "Germany");
            Assert.AreEqual(localeNames.FindCountryName("CZ"), "Czechia");
            Assert.AreEqual(localeNames.FindCountryName("GB"), "United Kingdom");
            Assert.AreEqual(localeNames.FindCountryName("unknown code"), null);
        }

        [TestMethod]
        public void Find_all_variants_of_country_name_by_code()
        {
            var localeNames = LocaleTranslations.ForLanguageCode("en-US");
            var result = localeNames.FindCountryNames("CZ");

            Assert.IsTrue(result.Count == 2, "CZ country name has only two variants");
            Assert.IsTrue(result[Enumerations.AltVariant.Common] == "Czechia");
            Assert.IsTrue(result[Enumerations.AltVariant.Alternative] == "Czech Republic");
        }
    }
}
