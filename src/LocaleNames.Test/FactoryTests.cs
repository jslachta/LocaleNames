using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LocaleNames.Test
{
    [TestClass]
    public class FactoryTests
    {

        /// <summary>
        /// Use the en-US culture when the InvariantCulture is given.
        /// </summary>
        [TestMethod]
        public void LocaleNames_Factory_ForCultureInfo_Test()
        {
            var translations = LocaleTranslations.ForCultureInfo(new System.Globalization.CultureInfo("en-US"));

            Assert.AreEqual(translations.CultureInfo, new System.Globalization.CultureInfo("en-US"));

            /*
             * When the invariant culture is given, the default "en-us"
             */ 

            translations = LocaleTranslations.ForCultureInfo(CultureInfo.InvariantCulture);

            Assert.AreEqual(translations.CultureInfo, new System.Globalization.CultureInfo("en-US"));
        }

        /// <summary>
        /// Locales the names factory test.
        /// </summary>
        [TestMethod]
        public void LocaleNames_Factory_ForLanguageCode_Test()
        {
            /*
             * if a valid language code is given, the cultureinfo name should match.
             */ 
            var translations = LocaleTranslations.ForLanguageCode("cs-CZ");

            Assert.AreEqual(translations.CultureInfo, new System.Globalization.CultureInfo("cs-CZ"));

            /*
             * if a fallback culture is given, no exception is thrown.
             */ 

            translations = LocaleTranslations.ForLanguageCode("non-existing-code", new CultureInfo("cs-CZ"));

            Assert.AreEqual(translations.CultureInfo, new System.Globalization.CultureInfo("cs-CZ"));
            
            /*
             * Without fallback cultureinfo the exception is thrown.
             */ 

            Assert.ThrowsException<CultureNotFoundException>(() => LocaleTranslations.ForLanguageCode("non-existing-code"));
        }
    }
}
