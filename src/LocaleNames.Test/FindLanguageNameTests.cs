using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace LocaleNames.Test
{
    /// <summary>
    /// FindLanguageName tests.
    /// </summary>
    [TestClass]
    public class FindLanguageNameTests
    {
        /// <summary>
        /// Finds the language name for given culture.
        /// </summary>
        [TestMethod]
        public void Find_language_name_for_given_culture()
        {
            var localeNames = LocaleTranslationsFactory.ForCultureInfo(new CultureInfo("cs-CZ"));

            Assert.AreEqual("čeština", localeNames.FindLanguageName("cs"));

            Assert.AreEqual("čeština", localeNames.FindLanguageName("cs-CZ"));

            Assert.AreEqual("angličtina (USA)", localeNames.FindLanguageName("en-US"));

            Assert.AreEqual("němčina (walser)", localeNames.FindLanguageName("wae"));

            Assert.AreEqual("čínština (tradiční)", localeNames.FindLanguageName("zh-Hant"));

            Assert.AreEqual(null, localeNames.FindLanguageName("unknown code"));

            localeNames = LocaleTranslationsFactory.ForCultureInfo(new CultureInfo("yue"));

            Assert.AreEqual("德文", localeNames.FindLanguageName("de"));

            Assert.AreEqual("捷克文", localeNames.FindLanguageName("cs"));

            Assert.AreEqual("英文 (美國)", localeNames.FindLanguageName("en-US"));
        }

        /// <summary>
        /// Finds the language name for current culture.
        /// </summary>
        [TestMethod]
        public void Find_language_name_for_current_culture()
        {
            var localeNames = LocaleTranslationsFactory.ForCurrentCulture();

            Assert.AreEqual("angličtina (USA)", localeNames.FindLanguageName("en-US"));

            Assert.AreEqual("němčina (walser)", localeNames.FindLanguageName("wae"));

            Assert.AreEqual("čínština (tradiční)", localeNames.FindLanguageName("zh-Hant"));
        }

        /// <summary>
        /// Finds the language name for language code.
        /// </summary>
        [TestMethod]
        public void Find_language_name_for_language_code()
        {
            var localeNames = LocaleTranslationsFactory.ForLanguageCode("en-US");

            Assert.AreEqual("German", localeNames.FindLanguageName("de"));

            Assert.AreEqual("Czech", localeNames.FindLanguageName("cs"));

            Assert.AreEqual("Czech", localeNames.FindLanguageName("cs-CZ"));

            Assert.AreEqual("American English", localeNames.FindLanguageName("en-US"));

            Assert.AreEqual("Chinese, Cantonese", localeNames.FindLanguageName("yue", Enumerations.AltVariant.Menu));
        }

        /// <summary>
        /// Finds the language name for unknown language code.
        /// </summary>
        [TestMethod]
        public void Find_language_name_for_unknown_language_code()
        {
            var localeNames = LocaleTranslationsFactory.ForLanguageCode("unknown-CODE");

            Assert.AreEqual(null, localeNames.FindLanguageName("de"));
        }
    }
}
