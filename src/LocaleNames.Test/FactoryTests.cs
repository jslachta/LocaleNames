using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace LocaleNames.Test
{
    /// <summary>
    /// <see cref="LocaleTranslationsFactory"/> tests.
    /// </summary>
    [TestClass]
    public class FactoryTests
    {
        /// <summary>
        /// On Windows the LocaleNames should have invariant CultureInfo.
        /// </summary>
        [TestMethod]
        public void Factory_ForLanguageCode_On_Windows_Should_Have_Invariant_Culture()
        {
            var localeNames = LocaleTranslationsFactory.ForLanguageCode("unknown code");

            if (OperatingSystem.IsWindows())
            {
                Assert.IsTrue(localeNames.CultureInfo == CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Use the en-US culture when the InvariantCulture is given.
        /// </summary>
        [TestMethod]
        public void Factory_ForCultureInfo_Existing_Culture_Test()
        {
            var translations = LocaleTranslationsFactory.ForCultureInfo(new System.Globalization.CultureInfo("en-US"));

            Assert.AreEqual(new System.Globalization.CultureInfo("en-US"), translations.CultureInfo);
            Assert.IsFalse(translations.AreCountryNameTranslationsEmpty);
            Assert.IsFalse(translations.AreLanguageTranslationsEmpty);
        }

        /// <summary>
        /// Locales the names factory test.
        /// </summary>
        [TestMethod]
        public void Factory_ForLanguageCode_Test()
        {
            /*
             * if a valid language code is given, the cultureinfo name should match.
             */
            var translations = LocaleTranslationsFactory.ForLanguageCode("cs-CZ");

            Assert.AreEqual(new System.Globalization.CultureInfo("cs-CZ"), translations.CultureInfo);
            Assert.IsFalse(translations.AreCountryNameTranslationsEmpty);
            Assert.IsFalse(translations.AreLanguageTranslationsEmpty);
        }

        /// <summary>
        /// For an unknown language code translations has invariant culture.
        /// </summary>
        [TestMethod]
        public void Factory_ForLanguageCode_InvalidLanguageCode_Test()
        {
            /*
             * if a not valid language code is given, the LocaleTranslations will not have any translations.
             */
            var translations = LocaleTranslationsFactory.ForLanguageCode("non-existing-code");

            Assert.IsTrue(translations.AreCountryNameTranslationsEmpty);
            Assert.IsTrue(translations.AreLanguageTranslationsEmpty);
        }
    }
}
