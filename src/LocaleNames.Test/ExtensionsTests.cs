using LocaleNames.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace LocaleTranslations.Test
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

            // 001 - World
            Assert.IsTrue(StringExtensions.IsCountryCodeContinent("001"));

            // 002 - Africa 
            Assert.IsTrue(StringExtensions.IsCountryCodeContinent("001"));

            // 150 - Europe
            Assert.IsTrue(StringExtensions.IsCountryCodeContinent("150"));

            // Return false on country
            Assert.IsFalse(StringExtensions.IsCountryCodeContinent("CZ"));

            // Return false on empty
            Assert.IsFalse(StringExtensions.IsCountryCodeContinent(String.Empty));

            // Return false on null
            Assert.IsFalse(StringExtensions.IsCountryCodeContinent(null));
        }
    }
}
