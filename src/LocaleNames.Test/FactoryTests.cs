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
        public void LocaleNames_Factory_ForCultureInfo_Existing_Culture_Test()
        {
            var translations = LocaleTranslations.ForCultureInfo(new System.Globalization.CultureInfo("en-US"));

            Assert.AreEqual(new System.Globalization.CultureInfo("en-US"), translations.CultureInfo);
            Assert.IsFalse(translations.AreCountryNameTranslationsEmpty);
            Assert.IsFalse(translations.AreLanguageTranslationsEmpty);
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

            Assert.AreEqual(new System.Globalization.CultureInfo("cs-CZ"), translations.CultureInfo);
            Assert.IsFalse(translations.AreCountryNameTranslationsEmpty);
            Assert.IsFalse(translations.AreLanguageTranslationsEmpty);
        }

        /// <summary>
        /// For an unknown language code translations has invariant culture.
        /// </summary>
        [TestMethod]
        public void LocaleNames_Factory_ForLanguageCode_InvalidLanguageCode_Test()
        {
            /*
             * if a not valid language code is given, the LocaleTranslations will not have any translations.
             */
            var translations = LocaleTranslations.ForLanguageCode("non-existing-code");

            Assert.IsTrue(translations.AreCountryNameTranslationsEmpty);
            Assert.IsTrue(translations.AreLanguageTranslationsEmpty);
        }
    }
}
