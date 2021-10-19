using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using LocaleNames;
using System.Linq;
using LocaleNames.Extensions;

namespace LocaleNames.Test
{
    [TestClass]
    public class ExtensionsTests
    {
        [TestMethod]
        public void LocaleNames_IsCountryCodeContinent()
        {
            /*
             * CLDR locale data contains continents in country codes.
             * 
             * It looks like that continents has a numerical code.
             */

            var codes = LocaleNames.LocaleTranslations.ForCurrentCulture().AllCountryCodes;

            // 001 - World
            Assert.IsTrue(StringExtensions.IsCountryCodeContinent("001"));

            // 002 - Africa 
            Assert.IsTrue(StringExtensions.IsCountryCodeContinent("001"));

            // 150 - Europe
            Assert.IsTrue(StringExtensions.IsCountryCodeContinent("150"));

            // Return false on country
            Assert.IsFalse(StringExtensions.IsCountryCodeContinent("CZ"));
        }
    }
}
